using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wedding_House.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Wedding_House.Controllers
{
    // Kế thừa Controller giống hệt các file đã chạy thành công trước đó
    public class PublicDataController : Controller
    {
        private readonly WeddingHouseContext _context;

        public PublicDataController(WeddingHouseContext context)
        {
            _context = context;
        }

        // 🌟 Định nghĩa thẳng URL đường link tuyệt đối
        [HttpGet("/api/PublicData/saloons")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSaloons()
        {
            try
            {
                // Dùng Select ngay từ đầu để tránh load dư cột gây crash EF Core
                var danhSachSanh = await _context.Sanhs
                    .Select(s => new {
                        s.MaSanh,
                        s.TenSanh,
                        s.LoaiSanh
                    })
                    .ToListAsync();

                var result = danhSachSanh.Select(s => new
                {
                    maSanh = s.MaSanh,
                    tenSanh = s.TenSanh,
                    loaiSanh = s.LoaiSanh,

                    sucChua = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? 550 : (s.LoaiSanh == "B" ? 350 : 200),
                    dienTich = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? 1200 : (s.LoaiSanh == "B" ? 850 : 520),
                    giaThue = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? 45000000 : (s.LoaiSanh == "B" ? 28000000 : 18000000),

                    hinhanh = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&q=80"
                            : (s.LoaiSanh == "B" ? "https://images.unsplash.com/photo-1519741497674-611481863552?w=800&q=80"
                            : "https://images.unsplash.com/photo-1550005809-91ad75fb315f?w=800&q=80")
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Ném thẳng lỗi chi tiết ra ngoài để bắt bệnh Database
                return StatusCode(500, new { message = "Lỗi Database Sảnh: " + ex.Message, inner = ex.InnerException?.Message });
            }
        }

        [HttpGet("/api/PublicData/dishes")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDishes()
        {
            try
            {
                var danhSachMon = await _context.Monans
                    .Select(m => new { m.MaMonAn, m.TenMonAn })
                    .ToListAsync();

                var result = danhSachMon.Select((m, index) => new
                {
                    maMonAn = m.MaMonAn,
                    tenMon = m.TenMonAn,
                    loaiMon = index % 4 == 0 ? "Khai vị" : (index % 4 == 3 ? "Tráng miệng" : "Món chính"),
                    moTa = "Hương vị thượng hạng được tinh tuyển bởi đội ngũ đầu bếp trưởng giàu kinh nghiệm của Wedding House.",
                    donGia = index % 4 == 0 ? 180000 : (index % 4 == 1 ? 450000 : (index % 4 == 2 ? 380000 : 150000)),
                    hinhanh = index % 4 == 0 ? "https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=600&q=80"
                            : (index % 4 == 1 ? "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=600&q=80"
                            : (index % 4 == 2 ? "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=600&q=80"
                            : "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600&q=80"))
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Database Món Ăn: " + ex.Message });
            }
        }

        [HttpGet("/api/PublicData/services")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServices()
        {
            try
            {
                var danhSachDichVu = await _context.Dichvus
                    .Select(d => new { d.MaDichVu, d.TenDichVu })
                    .ToListAsync();

                var result = danhSachDichVu.Select((d, index) => new {
                    maDv = d.MaDichVu,
                    tenDv = d.TenDichVu,
                    donGia = index % 3 == 0 ? 3000000 : (index % 3 == 1 ? 5000000 : 1500000)
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi Database Dịch vụ: " + ex.Message });
            }
        }
    }
}