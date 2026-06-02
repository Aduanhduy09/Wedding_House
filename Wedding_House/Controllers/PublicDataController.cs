using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicDataController : ControllerBase
    {
        private readonly WeddingHouseContext _context;

        public PublicDataController(WeddingHouseContext context)
        {
            _context = context;
        }

        // 1. API: Lấy danh sách sảnh tiệc (Đã map theo bảng SANH thực tế của bạn)
        [HttpGet("saloons")]
        public async Task<IActionResult> GetSaloons()
        {
            // Truy vấn đúng bảng Sanhs (hoặc Sanh) thực tế trong DB của bạn
            var danhSachSanh = await _context.Sanhs.ToListAsync();

            // Sử dụng LINQ Select để tự động đắp thêm các thuộc tính mở rộng cho UI
            var result = danhSachSanh.Select(s => new
            {
                maSanh = s.MaSanh,
                tenSanh = s.TenSanh,
                loaiSanh = s.LoaiSanh,
                ghiChu = s.GhiChu,

                // 🌟 TỰ ĐỘNG ĐẮP DATA ẢO: Dựa vào "LoaiSanh" để sinh ra sức chứa, diện tích và giá thuê tương ứng
                sucChua = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? 550 : (s.LoaiSanh == "B" ? 350 : 200),
                dienTich = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? 1200 : (s.LoaiSanh == "B" ? 850 : 520),
                giaThue = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? 45000000 : (s.LoaiSanh == "B" ? 28000000 : 18000000),

                // Gán link ảnh tương ứng cho từng loại sảnh tiệc
                hinhanh = s.LoaiSanh == "A" || s.LoaiSanh == "VVIP" ? "https://images.unsplash.com/photo-1464366400600-7168b8af9bc3?w=800&q=80"
                        : (s.LoaiSanh == "B" ? "https://images.unsplash.com/photo-1519741497674-611481863552?w=800&q=80"
                        : "https://images.unsplash.com/photo-1550005809-91ad75fb315f?w=800&q=80")
            });

            return Ok(result);
        }

        // 2. API: Lấy danh sách món ăn (Đã map theo bảng MONAN thực tế của bạn)
        [HttpGet("dishes")]
        public async Task<IActionResult> GetDishes()
        {
            // Truy vấn đúng bảng Monans (hoặc Monan) thực tế trong DB của bạn
            var danhSachMon = await _context.Monans.ToListAsync();

            // Tự động bù đắp thông tin phân loại, giá cả và hình ảnh để hiển thị đẹp mắt ngoài trang chủ
            var result = danhSachMon.Select((m, index) => new
            {
                maMonAn = m.MaMonAn,
                tenMon = m.TenMonAn,

                // 🌟 TỰ ĐỘNG ĐẮP DATA ẢO: Luân phiên phân bổ loại món ăn và giá cả dựa trên vị trí index của vòng lặp
                loaiMon = index % 4 == 0 ? "Khai vị" : (index % 4 == 3 ? "Tráng miệng" : "Món chính"),
                moTa = "Hương vị thượng hạng được tinh tuyển bởi đội ngũ đầu bếp trưởng giàu kinh nghiệm của Wedding House.",
                donGia = index % 4 == 0 ? 180000 : (index % 4 == 1 ? 450000 : (index % 4 == 2 ? 380000 : 150000)),

                // Luân phiên thay đổi 4 bức ảnh món ăn chất lượng cao trên trang chủ
                hinhanh = index % 4 == 0 ? "https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=600&q=80"
                        : (index % 4 == 1 ? "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=600&q=80"
                        : (index % 4 == 2 ? "https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=600&q=80"
                        : "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=600&q=80"))
            });

            return Ok(result);
        }
        [HttpGet("/api/PublicData/services")]
        public async Task<IActionResult> GetServices()
        {
            // Bốc dữ liệu từ bảng Dịch Vụ: Mã DV, Tên DV, Đơn giá mặc định
            var dichVus = await _context.Dichvus.Select(d => new {
                maDv = d.MaDichVu,
                tenDv = d.TenDichVu,
                donGia = d.DonGiaMacDinh // Đảm bảo tên cột khớp với Model của bạn
            }).ToListAsync();

            return Ok(dichVus);
        }
    }
}