using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    public class TaikhoanController : Controller
    {
        private readonly WeddingHouseContext _context;

        public TaikhoanController(WeddingHouseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult XacThucGmail() => View();

        [HttpGet("/api/Taikhoan/get-profile")]
        public async Task<IActionResult> GetProfile(string username)
        {
            var taiKhoan = await _context.Taikhoans.FirstOrDefaultAsync(t => t.TenDangNhap == username);
            if (taiKhoan == null) return NotFound(new { message = "Không tìm thấy tài khoản" });

            var khachHang = await _context.Khachhangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == taiKhoan.MaTaiKhoan);
            if (khachHang == null) return NotFound(new { message = "Hồ sơ khách hàng chưa được tạo" });

            return Ok(new
            {
                maKhachHang = khachHang.MaKhachHang,
                hoTen = khachHang.HoTen,
                dienThoai = khachHang.DienThoai,
                email = khachHang.Email
            });
        }

        [HttpGet("/api/Taikhoan/history")]
        public async Task<IActionResult> GetBookingHistory(string username)
        {
            var taiKhoan = await _context.Taikhoans.FirstOrDefaultAsync(t => t.TenDangNhap == username);
            if (taiKhoan == null) return BadRequest("Tài khoản không tồn tại.");

            var khachHang = await _context.Khachhangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == taiKhoan.MaTaiKhoan);
            if (khachHang == null) return Ok(new List<object>());

            // ĐÃ CẬP NHẬT: Kết nối sang bảng HOADON và lấy các thông tin cần thiết
            var history = await _context.Tieccuois
                .Where(t => t.MaKhachHang == khachHang.MaKhachHang)
                .OrderByDescending(t => t.NgayDaiTiec)
                .Select(t => new {
                    maDatTiecCuoi = t.MaDatTiecCuoi,
                    ngayDaiTiec = t.NgayDaiTiec,
                    maSanh = t.MaSanh,
                    maCa = t.MaCa,
                    tenChuRe = t.TenChuRe,
                    tenCoDau = t.TenCoDau,
                    // Lấy ngày lập từ đơn tiệc để tính phạt
                    ngayLap = t.NgayLap,
                    // Lấy trạng thái từ đơn tiệc (nếu có cột này trong DB)
                    trangThai = t.TrangThai,

                    // Truy vấn dữ liệu tài chính từ bảng Hóa Đơn
                    tongTien = _context.Hoadons
                        .Where(h => h.MaDatTiecCuoi == t.MaDatTiecCuoi)
                        .Select(h => h.TongTienHoaDon)
                        .FirstOrDefault(),
                    tienDaCoc = _context.Hoadons
                        .Where(h => h.MaDatTiecCuoi == t.MaDatTiecCuoi)
                        .Select(h => h.TienDatCoc)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(history);
        }
        // Mở đường dẫn hiển thị trang Quên Mật Khẩu
        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }
    }
}