using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly WeddingHouseContext _context;

        public BookingController(WeddingHouseContext context)
        {
            _context = context;
        }

        // =======================================================
        // API 1: KIỂM TRA TRÙNG LỊCH TRỐNG CỦA SẢNH TIỆC
        // =======================================================
        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability(int maSanh, DateTime ngayToChuc, int maCa)
        {
            var ngay = DateOnly.FromDateTime(ngayToChuc);

            var isBusy = await _context.Tieccuois.AnyAsync(b =>
                b.MaSanh == maSanh &&
                b.NgayDaiTiec == ngay &&
                b.MaCa == maCa);

            if (isBusy)
            {
                return BadRequest(new { message = "Rất tiếc, sảnh này vào ngày và ca bạn chọn đã có cặp đôi khác đặt trước!" });
            }

            return Ok(new { message = "Sảnh còn trống! Bạn có thể tiếp tục đặt tiệc." });
        }

        // =======================================================
        // API 2: TIẾP NHẬN ĐƠN, LƯU MÓN ĂN, DỊCH VỤ VÀ HÓA ĐƠN
        // =======================================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
        {
            if (request.DanhSachMaMonAn == null || !request.DanhSachMaMonAn.Any())
            {
                return BadRequest(new { message = "Vui lòng chọn ít nhất một món ăn cho thực đơn tiệc cưới!" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ngay = DateOnly.FromDateTime(request.NgayToChuc);
                var isBusy = await _context.Tieccuois.AnyAsync(b =>
                    b.MaSanh == request.MaSanh &&
                    b.NgayDaiTiec == ngay &&
                    b.MaCa == request.MaCa);

                if (isBusy)
                {
                    return BadRequest(new { message = "Lịch sảnh đã bị trùng, vui lòng kiểm tra lại!" });
                }

                // 1. LƯU BẢNG TIECCUOI
                var donDatTiec = new Tieccuoi
                {
                    MaKhachHang = request.MaKhachHang,
                    MaSanh = request.MaSanh,
                    NgayDaiTiec = ngay,
                    MaCa = request.MaCa,
                    SoLuongBan = request.SoLuongBan,
                    TienDatCoc = 0, // 🌟 Sửa lại: Khách mới đặt chưa nộp tiền nên cọc = 0
                    TenChuRe = request.TenChuRe ?? string.Empty,
                    TenCoDau = request.TenCoDau ?? string.Empty
                    // NgayLap = DateTime.Now // Bỏ comment nếu Model Tieccuoi của bạn đã có cột NgayLap
                };

                _context.Tieccuois.Add(donDatTiec);
                await _context.SaveChangesAsync();

                // 2. LƯU BẢNG CHITIETDATMON
                foreach (var maMon in request.DanhSachMaMonAn)
                {
                    var chiTietMon = new Chitietdatmon
                    {
                        MaDatTiecCuoi = donDatTiec.MaDatTiecCuoi,
                        MaMonAn = maMon,
                        DonGia = request.DonGiaTamTinhMoiMon,
                        GhiChu = "Khách tự chọn qua hệ thống"
                    };
                    _context.Chitietdatmons.Add(chiTietMon);
                }

                // 3. LƯU BẢNG CHITIETDICHVU (MỚI THÊM)
                if (request.DanhSachMaDichVu != null && request.DanhSachMaDichVu.Any())
                {
                    foreach (var maDv in request.DanhSachMaDichVu)
                    {
                        var dichVu = await _context.Dichvus.FindAsync(maDv);
                        if (dichVu != null)
                        {
                            var chiTietDv = new Chitietdichvu
                            {
                                MaDatTiecCuoi = donDatTiec.MaDatTiecCuoi,
                                MaDichVu = maDv,
                                SoLuong = 1,
                                DonGia = dichVu.DonGiaMacDinh,
                                ThanhTien = dichVu.DonGiaMacDinh * 1
                            };
                            _context.Chitietdichvus.Add(chiTietDv);
                        }
                    }
                }

                // Lưu Món ăn và Dịch vụ xuống DB
                await _context.SaveChangesAsync();

                // 4. LƯU BẢNG HOADON NHÁP (MỚI THÊM)
                decimal tongTienBan = request.SoLuongBan * (request.DonGiaTamTinhMoiMon * request.DanhSachMaMonAn.Count);
                decimal tongTienDichVu = request.TongTien - tongTienBan;

                var hoaDonMoi = new Hoadon
                {
                    MaDatTiecCuoi = donDatTiec.MaDatTiecCuoi,
                    TongTienBan = tongTienBan,
                    TongTienDichVu = tongTienDichVu,
                    TongTienHoaDon = request.TongTien,
                    TienDatCoc = 0, // Cọc = 0
                    TienConLai = request.TongTien, // Nợ 100%
                    TienPhat = 0
                    // NgayLapHoaDon = DateTime.Now // Bỏ comment nếu Model Hoadon của bạn đã có cột NgayLapHoaDon
                };

                _context.Hoadons.Add(hoaDonMoi);
                await _context.SaveChangesAsync();

                // 5. CHỐT GIAO DỊCH 
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Gửi đơn đặt tiệc thành công! Wedding House sẽ liên hệ xác nhận sớm nhất.",
                    maDatTiec = donDatTiec.MaDatTiecCuoi
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rút lại toàn bộ nếu có bất kỳ lỗi nào
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // =======================================================
        // API 3: LẤY CHI TIẾT ĐƠN ĐỂ ĐỔ LÊN TRANG BIÊN LAI ĐỘNG
        // =======================================================
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            try
            {
                // 1. Lấy thông tin tiệc cưới (Dùng Select để tránh lỗi mapping dư cột)
                var tiec = await _context.Tieccuois
                    .Where(t => t.MaDatTiecCuoi == id)
                    .Select(t => new {
                        t.MaDatTiecCuoi,
                        t.MaSanh,
                        t.NgayDaiTiec,
                        t.MaCa,
                        t.SoLuongBan
                    })
                    .FirstOrDefaultAsync();

                if (tiec == null) return NotFound(new { message = "Không tìm thấy đơn đặt tiệc cưới!" });

                // 2. Lấy tên sảnh (Chỉ bốc cột TenSanh để né triệt để lỗi bảng Sanh)
                var sanh = await _context.Sanhs
                    .Where(s => s.MaSanh == tiec.MaSanh)
                    .Select(s => new { s.TenSanh })
                    .FirstOrDefaultAsync();

                // 3. Lấy hóa đơn
                var hoaDon = await _context.Hoadons
                    .Where(h => h.MaDatTiecCuoi == id)
                    .Select(h => new {
                        h.TongTienBan,
                        h.TongTienDichVu,
                        h.TongTienHoaDon
                    })
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    maDon = tiec.MaDatTiecCuoi,
                    tenSanh = sanh?.TenSanh ?? "Sảnh Tiệc Cao Cấp",
                    ngayCuoi = tiec.NgayDaiTiec.ToString("dd/MM/yyyy"),
                    ca = tiec.MaCa == 1 ? "Buổi Trưa" : "Buổi Tối",
                    soBan = tiec.SoLuongBan,

                    // Lấy số liệu trực tiếp từ Hóa Đơn thay vì gọi từ bảng Sảnh
                    tienSanh = hoaDon?.TongTienDichVu ?? 0m,
                    tienMonAn = hoaDon?.TongTienBan ?? 0m,
                    tongTien = hoaDon?.TongTienHoaDon ?? 0m
                });
            }
            catch (Exception ex)
            {
                // Trả về lỗi chi tiết để nếu có lỗi cũng không bị sụp web ngầm
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết biên lai: " + ex.Message });
            }
        }
        // =======================================================
        //  API 4: HỦY ĐƠN ĐẶT TIỆC VÀ XÓA TOÀN BỘ DỮ LIỆU LIÊN QUAN
        // =======================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            // Kích hoạt giao dịch an toàn
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tìm kiếm đơn tiệc cưới gốc
                var tiec = await _context.Tieccuois.FirstOrDefaultAsync(t => t.MaDatTiecCuoi == id);
                if (tiec == null) return NotFound(new { message = "Không tìm thấy đơn đặt tiệc cần hủy!" });

                // 1. Xóa dữ liệu trong bảng HOADON liên quan đến đơn này
                var hoaDons = _context.Hoadons.Where(h => h.MaDatTiecCuoi == id);
                _context.Hoadons.RemoveRange(hoaDons);

                // 2. Xóa dữ liệu trong bảng CHITIETDICHVU liên quan đến đơn này
                var chiTietDVs = _context.Chitietdichvus.Where(ct => ct.MaDatTiecCuoi == id);
                _context.Chitietdichvus.RemoveRange(chiTietDVs);

                // 3. Xóa dữ liệu trong bảng CHITIETDATMON (Món ăn) liên quan đến đơn này
                var chiTietMonAns = _context.Chitietdatmons.Where(ct => ct.MaDatTiecCuoi == id);
                _context.Chitietdatmons.RemoveRange(chiTietMonAns);

                // 4. Cuối cùng, tiến hành xóa bản ghi gốc ở bảng TIECCUOI
                _context.Tieccuois.Remove(tiec);

                // Lưu toàn bộ thay đổi xuống SQL Server cùng một lúc
                await _context.SaveChangesAsync();

                // Xác nhận chốt giao dịch thành công
                await transaction.CommitAsync();

                return Ok(new { message = "Hủy đơn và dọn dẹp dữ liệu thành công." });
            }
            catch (Exception ex)
            {
                // Nếu có bất kỳ trục trặc nào, hoàn tác toàn bộ để tránh mất mát dữ liệu
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống khi thực hiện xóa: " + ex.Message });
            }
        }
        // =======================================================
        // API 5: KHÁCH HÀNG BÁO CÁO ĐÃ CHUYỂN KHOẢN CỌC (CHỜ ADMIN DUYỆT)
        // =======================================================
        [HttpPost("confirm-payment/{id}")]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            try
            {
                // Tìm đơn đặt tiệc cưới dựa vào mã ID
                var tiec = await _context.Tieccuois.FirstOrDefaultAsync(t => t.MaDatTiecCuoi == id);

                if (tiec == null)
                    return NotFound(new { message = "Không tìm thấy đơn đặt tiệc cưới này!" });

                // Cập nhật trạng thái thành 9 (Đang xử lý / Chờ kiểm tra sao kê)
                tiec.TrangThai = 9;

                // Lưu thay đổi xuống SQL Server
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã gửi thông báo chuyển khoản thành công! Wedding House sẽ kiểm tra sao kê ngân hàng và phê duyệt trong ít phút." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật trạng thái thanh toán: " + ex.Message });
            }
        }
        // =======================================================
        //  API ADMIN 1: LẤY DANH SÁCH ĐƠN CHỜ DUYỆT CỌC (TrangThai == 9)
        // =======================================================
        [HttpGet("admin/pending-deposits")]
        public async Task<IActionResult> GetPendingDeposits()
        {
            try
            {
                var pendingList = await _context.Tieccuois
                    .Where(t => t.TrangThai == 9)
                    .OrderBy(t => t.NgayLap) // Đơn nào đặt trước hiện lên trước để duyệt
                    .Select(t => new {
                        maDatTiecCuoi = t.MaDatTiecCuoi,
                        tenKhachHang = _context.Khachhangs.Where(k => k.MaKhachHang == t.MaKhachHang).Select(k => k.HoTen).FirstOrDefault(),
                        coDauChuRe = t.TenCoDau + " - " + t.TenChuRe,
                        sanhCa = "Sảnh " + t.MaSanh + " / Ca " + (t.MaCa == 1 ? "Trưa" : "Tối"),
                        ngayDaiTiec = t.NgayDaiTiec.ToString("dd/MM/yyyy"),
                        ngayLap = t.NgayLap,
                        tongTien = _context.Hoadons.Where(h => h.MaDatTiecCuoi == t.MaDatTiecCuoi).Select(h => h.TongTienHoaDon).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(pendingList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi load danh sách chờ duyệt: " + ex.Message });
            }
        }

        // =======================================================
        // API ADMIN 2: PHÊ DUYỆT ĐÃ NHẬN TIỀN CỌC (Cập nhật HOADON thật)
        // =======================================================
        [HttpPost("admin/approve-deposit/{id}")]
        public async Task<IActionResult> ApproveDeposit(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tiec = await _context.Tieccuois.FirstOrDefaultAsync(t => t.MaDatTiecCuoi == id);
                if (tiec == null) return NotFound(new { message = "Không tìm thấy đơn tiệc cưới!" });

                var hoaDon = await _context.Hoadons.FirstOrDefaultAsync(h => h.MaDatTiecCuoi == id);
                if (hoaDon == null) return NotFound(new { message = "Không tìm thấy hóa đơn tương ứng!" });

                // 1. Chuyển trạng thái tiệc cưới sang Đã thanh toán cọc (1)
                tiec.TrangThai = 1;

                // 2. Tính toán số ngày trễ hạn thực tế tại thời điểm duyệt để chốt con số xuống DB
                var ngayLapDon = tiec.NgayLap;
                var ngayHienTai = DateTime.Now;
                var diffDays = (ngayHienTai - ngayLapDon).Days;
                int soNgayTre = diffDays > 1 ? diffDays - 1 : 0;

                // Tính tiền phạt (1%/ngày trễ) và tiền cọc gốc (20%)
                decimal tienPhat = soNgayTre * ((hoaDon.TongTienHoaDon ?? 0m) * 0.01m);
                decimal tienCocGoc = (hoaDon.TongTienHoaDon ?? 0m) * 0.20m;
                decimal tongTienCocThucThu = tienCocGoc + tienPhat;

                // 3. Ghi nhận dòng tiền thật vào bảng HOADON
                hoaDon.TienPhat = tienPhat;
                hoaDon.TienDatCoc = tongTienCocThucThu;

                // Tiền còn lại = (Tổng hóa đơn gốc + Tiền phạt phát sinh nếu có) - Tiền cọc đã thu
                hoaDon.TienConLai = (hoaDon.TongTienHoaDon + tienPhat) - tongTienCocThucThu;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = $"Phê duyệt thành công đơn #WHD-{id}! Hệ thống đã ghi nhận số tiền cọc: {tongTienCocThucThu.ToString("N0")} đ." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Lỗi hệ thống khi phê duyệt đơn: " + ex.Message });
            }
        }
        // =======================================================
        // 🌟 API ADMIN 3: LẤY DANH SÁCH TIỆC CƯỚI ĐỂ ĐIỀU PHỐI & TẤT TOÁN (ĐÃ SỬA)
        // =======================================================
        [HttpGet("admin/confirmed-bookings")]
        public async Task<IActionResult> GetConfirmedBookings()
        {
            try
            {
                var confirmedList = await _context.Tieccuois
                    // 🌟 LẤY CẢ ĐƠN ĐÃ CỌC (1) VÀ ĐƠN ĐÃ TẤT TOÁN (2) ĐỂ HIỂN THỊ TRÊN GIAO DIỆN
                    .Where(t => t.TrangThai == 1 || t.TrangThai == 2)
                    .OrderBy(t => t.NgayDaiTiec)
                    .Select(t => new {
                        maDatTiecCuoi = t.MaDatTiecCuoi,
                        tenKhachHang = _context.Khachhangs.Where(k => k.MaKhachHang == t.MaKhachHang).Select(k => k.HoTen).FirstOrDefault(),
                        coDauChuRe = t.TenCoDau + " - " + t.TenChuRe,
                        maSanh = t.MaSanh,
                        // 🌟 ĐÃ SỬA: Sửa s.MaSanh == t.MaSanh để bốc đúng tên sảnh từ DB
                        tenSanh = _context.Sanhs.Where(s => s.MaSanh == t.MaSanh).Select(s => s.TenSanh).FirstOrDefault(),
                        maCa = t.MaCa,
                        tenCa = t.MaCa == 1 ? "Buổi Trưa" : "Buổi Tối",
                        ngayDaiTiec = t.NgayDaiTiec.ToString("dd/MM/yyyy"),
                        soLuongBan = t.SoLuongBan,

                        // Trả thêm trạng thái để Frontend nhận biết render nút bấm hay tích xanh
                        trangThai = t.TrangThai,

                        tongTien = _context.Hoadons.Where(h => h.MaDatTiecCuoi == t.MaDatTiecCuoi).Select(h => h.TongTienHoaDon).FirstOrDefault(),
                        tienDaCoc = _context.Hoadons.Where(h => h.MaDatTiecCuoi == t.MaDatTiecCuoi).Select(h => h.TienDatCoc).FirstOrDefault(),
                        tienConLai = _context.Hoadons.Where(h => h.MaDatTiecCuoi == t.MaDatTiecCuoi).Select(h => h.TienConLai).FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(confirmedList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tải danh sách sảnh tiệc: " + ex.Message });
            }
        }
        // =======================================================
        // API ADMIN 4: XÁC NHẬN TẤT TOÁN HOÀN TẤT ĐƠN TIỆC (TrangThai == 2)
        // =======================================================
        [HttpPost("admin/checkout/{id}")]
        public async Task<IActionResult> CheckoutBooking(int id)
        {
            try
            {
                // 1. Tìm đơn đặt tiệc cưới theo ID truyền lên
                var tiec = await _context.Tieccuois.FirstOrDefaultAsync(t => t.MaDatTiecCuoi == id);
                if (tiec == null) return NotFound(new { message = "Không tìm thấy đơn tiệc cưới!" });

                // 2. Tìm hóa đơn tương ứng để xử lý dòng tiền còn lại
                var hoaDon = await _context.Hoadons.FirstOrDefaultAsync(h => h.MaDatTiecCuoi == id);
                if (hoaDon == null) return NotFound(new { message = "Không tìm thấy hóa đơn tương ứng!" });

                // 3. Cập nhật trạng thái sang 2 (Đã hoàn tất trọn vẹn hóa đơn)
                tiec.TrangThai = 2;

                // 4. Số tiền còn lại thu nốt chuyển về 0 vì khách đã tất toán xong
                hoaDon.TienConLai = 0;

                // Lưu thay đổi xuống SQL Server
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Đã xác nhận tất toán hoàn tất đơn hàng #WHD-{id} lên hệ thống thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tất toán hóa đơn: " + ex.Message });
            }
        }
        // =======================================================
        // API ADMIN 5: TÍNH TỔNG DOANH THU HOÁ ĐƠN TRONG THÁNG HIỆN TẠI
        // =======================================================
        [HttpGet("/api/Admin/monthly-revenue")]
        [Authorize(Roles = "Admin")] // Bảo mật Token của hệ thống Admin
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            try
            {
                // Lấy mốc thời gian thời điểm hiện tại 
                var ngayHienTai = DateTime.Now;
                var thangHienTai = ngayHienTai.Month;
                var namHienTai = ngayHienTai.Year;

                // Truy vấn tính tổng tiền: Kết hợp HOADON và TIECCUOI
                var tongDoanhThu = await _context.Hoadons
                    .Join(_context.Tieccuois,
                        hd => hd.MaDatTiecCuoi,
                        tc => tc.MaDatTiecCuoi,
                        (hd, tc) => new { Hoadon = hd, Tieccuoi = tc })
                    // Bộ lọc: Chỉ lấy những tiệc tổ chức trúng tháng và năm hiện tại
                    .Where(x => x.Tieccuoi.NgayDaiTiec.Month == thangHienTai &&
                                 x.Tieccuoi.NgayDaiTiec.Year == namHienTai)
                    .SumAsync(x => (decimal?)x.Hoadon.TongTienHoaDon) ?? 0;

                return Ok(new { revenue = tongDoanhThu });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tính doanh thu: " + ex.Message });
            }
        }
        // =======================================================
        // API ADMIN: ĐẾM SỐ LƯỢNG TIỆC SẮP TỚI (TỪ HÔM NAY TRỞ ĐI)
        // =======================================================
        [HttpGet("/api/Admin/upcoming-events")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            try
            {
                // 🌟 ĐÃ SỬA TẠI ĐÂY: Chuyển đổi DateTime.Today sang chuẩn DateOnly cho khớp với Database
                var homNay = DateOnly.FromDateTime(DateTime.Today);

                // Truy vấn bảng TIECCUOI: Đếm tất cả các tiệc có ngày đãi >= hôm nay
                var soLuongTiecSapToi = await _context.Tieccuois
                    .Where(tc => tc.NgayDaiTiec >= homNay)
                    .CountAsync();

                return Ok(new { count = soLuongTiecSapToi });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi đếm tiệc: " + ex.Message });
            }
        }
        // =======================================================
        // API ADMIN: ĐẾM TỔNG SỐ TÀI KHOẢN TRONG HỆ THỐNG
        // =======================================================
        [HttpGet("/api/Admin/total-accounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTotalAccounts()
        {
            try
            {
                // Đếm tổng số dòng trong bảng TAIKHOAN
                var tongTaiKhoan = await _context.Taikhoans.CountAsync();

                return Ok(new { count = tongTaiKhoan });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi đếm tài khoản: " + ex.Message });
            }
        }
        // =======================================================
        // 🌟 API ADMIN: LẤY CHI TIẾT TOÀN BỘ ĐƠN TIỆC ĐỂ HIỂN THỊ MODAL
        // =======================================================
        [HttpGet("admin/full-details/{id}")]
        public async Task<IActionResult> GetFullBookingDetails(int id)
        {
            try
            {
                // 1. Lấy thông tin cơ bản (Dùng Select để né lỗi EF Core)
                var tiec = await _context.Tieccuois
                    .Where(t => t.MaDatTiecCuoi == id)
                    .Select(t => new {
                        t.MaDatTiecCuoi,
                        t.MaSanh,
                        t.MaCa,
                        t.SoLuongBan,
                        t.NgayDaiTiec,
                        t.TenCoDau,
                        t.TenChuRe
                    })
                    .FirstOrDefaultAsync();

                if (tiec == null) return NotFound(new { message = "Không tìm thấy đơn!" });

                var sanh = await _context.Sanhs
                    .Where(s => s.MaSanh == tiec.MaSanh)
                    .Select(s => new { s.TenSanh })
                    .FirstOrDefaultAsync();

                var hoadon = await _context.Hoadons
                    .Where(h => h.MaDatTiecCuoi == id)
                    .Select(h => new {
                        h.TongTienBan,
                        h.TongTienDichVu,
                        h.TongTienHoaDon,
                        h.TienDatCoc,
                        h.TienConLai
                    })
                    .FirstOrDefaultAsync();

                // 2. Lấy danh sách Món ăn (Join CHITIETDATMON với MONAN)
                var dsMonAn = await _context.Chitietdatmons
                    .Where(ct => ct.MaDatTiecCuoi == id)
                    .Join(_context.Monans, ct => ct.MaMonAn, m => m.MaMonAn, (ct, m) => new {
                        tenMon = m.TenMonAn,
                        donGia = ct.DonGia
                    })
                    .ToListAsync();

                // 3. Lấy danh sách Dịch vụ (Join CHITIETDICHVU với DICHVU)
                var dsDichVu = await _context.Chitietdichvus
                    .Where(ct => ct.MaDatTiecCuoi == id)
                    .Join(_context.Dichvus, ct => ct.MaDichVu, d => d.MaDichVu, (ct, d) => new {
                        tenDv = d.TenDichVu,
                        thanhTien = ct.ThanhTien
                    })
                    .ToListAsync();

                // 4. Trả về gói dữ liệu tổng hợp
                return Ok(new
                {
                    maDon = tiec.MaDatTiecCuoi,
                    coDauChuRe = tiec.TenCoDau + " & " + tiec.TenChuRe,
                    sanhCa = (sanh?.TenSanh ?? "Sảnh") + " - Ca " + (tiec.MaCa == 1 ? "Trưa" : "Tối"),
                    soBan = tiec.SoLuongBan,
                    ngayDai = tiec.NgayDaiTiec.ToString("dd/MM/yyyy"),

                    danhSachMon = dsMonAn,
                    danhSachDichVu = dsDichVu,

                    taiChinh = new
                    {
                        tongTienBan = hoadon?.TongTienBan ?? 0,
                        tongTienDichVu = hoadon?.TongTienDichVu ?? 0,
                        tongHoaDon = hoadon?.TongTienHoaDon ?? 0,
                        tienDaCoc = hoadon?.TienDatCoc ?? 0,
                        tienConLai = hoadon?.TienConLai ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi load chi tiết tiệc: " + ex.Message });
            }
        }
        // =======================================================
        // API MỚI: KIỂM TRA SỨC CHỨA CỦA SẢNH TỪ BẢNG THAM SỐ
        // =======================================================
        [HttpGet("check-capacity")]
        public async Task<IActionResult> CheckCapacity(int maSanh, int soBanDuKien)
        {
            try
            {
                // 1. Dùng Select() để chỉ lấy đúng mã Loại Sảnh (Né lỗi bảng SANH nếu có)
                var loaiSanh = await _context.Sanhs
                    .Where(s => s.MaSanh == maSanh)
                    .Select(s => s.LoaiSanh)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(loaiSanh))
                    return NotFound(new { message = "Sảnh không tồn tại trên hệ thống!" });

                // 2. DÙNG .SELECT() BỌC THÉP: Chỉ bốc đúng 1 cột SoLuongBanToiDa từ bảng THAMSO
                // Lệnh này ép EF Core sinh ra SQL: SELECT SoLuongBanToiDa FROM THAMSO... 
                // Né hoàn toàn cột DonGiaBanToiThieu đang bị lỗi mapping!
                var soBanToiDa = await _context.Thamsos
                    .Where(t => t.LoaiSanh == loaiSanh)
                    .Select(t => t.SoLuongBanToiDa)
                    .FirstOrDefaultAsync();

                // 3. So sánh và chặn đứng nếu vi phạm
                if (soBanToiDa > 0 && soBanDuKien > soBanToiDa)
                {
                    return BadRequest(new
                    {
                        isValid = false,
                        message = $"Số lượng bàn tối đa là: {soBanToiDa}"
                    });
                }

                // Hợp lệ cho qua
                return Ok(new { isValid = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi kiểm tra sức chứa: " + ex.Message });
            }
        }
    }

    // =======================================================
    //  LỚP VẬN CHUYỂN DỮ LIỆU (DTO) ĐỒNG BỘ GIỮA FE VÀ BE
    // =======================================================
    public class BookingRequestDto
    {
        public int MaKhachHang { get; set; }
        public int MaSanh { get; set; }
        public DateTime NgayToChuc { get; set; }
        public int MaCa { get; set; }
        public string? TenChuRe { get; set; }
        public string? TenCoDau { get; set; }
        public int SoLuongBan { get; set; }
        public decimal TongTien { get; set; }
        public decimal DonGiaTamTinhMoiMon { get; set; }
        public List<int> DanhSachMaMonAn { get; set; } = new();

        // MỚI THÊM: Hứng danh sách ID dịch vụ từ Frontend
        public List<int> DanhSachMaDichVu { get; set; } = new();
    }
}