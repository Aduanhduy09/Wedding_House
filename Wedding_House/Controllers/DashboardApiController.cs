using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardApiController : ControllerBase
    {
        private readonly WeddingHouseContext _context;

        public DashboardApiController(WeddingHouseContext context)
        {
            _context = context;
        }

        // =======================================================
        // API: TÍNH TỔNG DOANH THU HOÁ ĐƠN TRONG THÁNG HIỆN TẠI (CHO ADMIN)
        // =======================================================
        [HttpGet("monthly-revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            try
            {
                var now = DateTime.Now;
                var month = now.Month;
                var year = now.Year;

                var tongDoanhThu = await _context.Hoadons
                    .Join(_context.Tieccuois,
                        hd => hd.MaDatTiecCuoi,
                        tc => tc.MaDatTiecCuoi,
                        (hd, tc) => new { Hoadon = hd, Tieccuoi = tc })
                    .Where(x => x.Tieccuoi.NgayDaiTiec.Month == month && x.Tieccuoi.NgayDaiTiec.Year == year)
                    .SumAsync(x => (decimal?)x.Hoadon.TongTienHoaDon) ?? 0m;

                return Ok(new { revenue = tongDoanhThu });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tính doanh thu: " + ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                // 1. Tính doanh thu tháng hiện tại (Tổng các hóa đơn đã thanh toán cọc hoặc hoàn tất trong tháng này)
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                decimal revenue = await _context.Hoadons
                    .Where(h => _context.Tieccuois
                        .Any(t => t.MaDatTiecCuoi == h.MaDatTiecCuoi && (t.TrangThai == 1 || t.TrangThai == 2) && t.NgayDaiTiec.Month == currentMonth && t.NgayDaiTiec.Year == currentYear))
                    .SumAsync(h => h.TienDatCoc ?? 0m); // Có thể cộng thêm tất toán tùy nghiệp vụ của bạn

                // 2. Đếm số tiệc cưới sắp diễn ra (Ngày đãi tiệc >= Ngày hiện tại và đơn hợp lệ)
                var todayDateOnly = DateOnly.FromDateTime(DateTime.Today);
                int upcomingEvents = await _context.Tieccuois
                    .CountAsync(t => t.NgayDaiTiec >= todayDateOnly && t.TrangThai != 0);

                // 3. Tính tỷ lệ lấp đầy sảnh tiệc (Ví dụ: Số ca đã đặt trong tháng / Tổng số ca có thể phục vụ)
                // Công thức mẫu trực quan: (Số đơn tiệc tháng này) / (Tổng số sảnh * Số ca mỗi ngày * Số ngày trong tháng)
                int totalBallrooms = await _context.Sanhs.CountAsync();
                int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);
                int totalCapacity = (totalBallrooms == 0 ? 1 : totalBallrooms) * 2 * daysInMonth; // 1 ngày có 2 ca (Trưa/Tối)
                int bookedSlots = await _context.Tieccuois.CountAsync(t => t.NgayDaiTiec.Month == currentMonth && t.NgayDaiTiec.Year == currentYear && t.TrangThai != 0);
                int occupancyRate = totalCapacity > 0 ? (bookedSlots * 100) / totalCapacity : 0;

                // 4. Đếm tổng số tài khoản hệ thống đang có
                int totalAccounts = await _context.Taikhoans.CountAsync();

                // 5. Bốc dữ liệu trạng thái sảnh tiệc thực tế hôm nay từ DB
                var todayWeddings = await _context.Tieccuois
                    .Where(t => t.NgayDaiTiec == todayDateOnly && t.TrangThai != 0)
                    .ToListAsync();

                var ballroomsStatus = await _context.Sanhs.Select(s => new {
                    tenSanh = s.TenSanh,
                    // Nếu sảnh có trong danh sách tiệc hôm nay thì là "busy", ngược lại là "available"
                    status = todayWeddings.Any(w => w.MaSanh == s.MaSanh) ? "busy" : "available",
                    note = todayWeddings.Where(w => w.MaSanh == s.MaSanh).Select(w => "Tiệc đám cưới chú rể - cô dâu").FirstOrDefault() ?? "Sẵn sàng đặt tiệc"
                }).ToListAsync();

                // Trả trọn bộ dữ liệu gói gọn về cho Frontend
                return Ok(new
                {
                    revenue = revenue.ToString("N0") + " đ",
                    upcomingEvents = upcomingEvents,
                    occupancyRate = occupancyRate + "%",
                    totalAccounts = totalAccounts,
                    ballrooms = ballroomsStatus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi tính toán số liệu dashboard: " + ex.Message });
            }
        }
    }
}