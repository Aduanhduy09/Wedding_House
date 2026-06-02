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
        // 🌟 API 1: KIỂM TRA TRÙNG LỊCH TRỐNG CỦA SẢNH TIỆC
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
        // 🌟 API 2: TIẾP NHẬN ĐƠN, LƯU MÓN ĂN, DỊCH VỤ VÀ HÓA ĐƠN
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
        // 🌟 API 3: LẤY CHI TIẾT ĐƠN ĐỂ ĐỔ LÊN TRANG BIÊN LAI ĐỘNG
        // =======================================================
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            var tiec = await _context.Tieccuois.FirstOrDefaultAsync(t => t.MaDatTiecCuoi == id);
            if (tiec == null) return NotFound(new { message = "Không tìm thấy đơn đặt tiệc cưới!" });

            var sanh = await _context.Sanhs.FirstOrDefaultAsync(s => s.MaSanh == tiec.MaSanh);

            // 🌟 ĐÃ SỬA: Lấy Hóa đơn nháp tương ứng với đơn tiệc này để lấy đúng số tiền
            var hoaDon = await _context.Hoadons.FirstOrDefaultAsync(h => h.MaDatTiecCuoi == id);

            // Lấy giá thuê sảnh thật từ database
            decimal giaThueSanh = sanh?.GiaThue ?? 0;

            // Lấy tiền bàn và tổng tiền từ bảng Hóa Đơn (Nếu không thấy thì trả về 0)
            decimal tienMonAn = hoaDon?.TongTienBan ?? 0m;
            decimal tongChiPhi = hoaDon?.TongTienHoaDon ?? 0m;

            return Ok(new
            {
                maDon = tiec.MaDatTiecCuoi,
                tenSanh = sanh?.TenSanh ?? "Sảnh Tiệc Cao Cấp",
                ngayCuoi = tiec.NgayDaiTiec.ToString("dd/MM/yyyy"),
                ca = tiec.MaCa == 1 ? "Buổi Trưa" : "Buổi Tối",
                soBan = tiec.SoLuongBan,
                tienSanh = giaThueSanh,
                tienMonAn = tienMonAn,
                tongTien = tongChiPhi
            });
        }
    }

    // =======================================================
    // 🌟 LỚP VẬN CHUYỂN DỮ LIỆU (DTO) ĐỒNG BỘ GIỮA FE VÀ BE
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