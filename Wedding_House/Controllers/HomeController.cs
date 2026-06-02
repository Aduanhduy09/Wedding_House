using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    public class HomeController : Controller
    {
        // 🌹 1. TRANG CHỦ CHO KHÁCH HÀNG (Đường dẫn: / hoặc /Home/Index)
        public IActionResult Index()
        {
            return View();
        }

        // 📊 2. TUYẾN ĐƯỜNG DỰ PHÒNG (Đường dẫn: /Home/Dashboard)
        // Nếu lỡ có chỗ nào gọi link cũ, lệnh này sẽ ép Server tìm đúng file "AdminDashboard" của bạn
        public IActionResult Dashboard()
        {
            return View("AdminDashboard");
        }

        // 🚀 3. TUYẾN ĐƯỜNG CHÍNH ĐỒNG BỘ (Đường dẫn: /Home/AdminDashboard)
        // Hàm này khớp hoàn toàn với tên file AdminDashboard.cshtml bạn đã tạo
        public IActionResult AdminDashboard()
        {
            return View(); // Tự động tìm file AdminDashboard.cshtml cùng tên với hàm
        }

        // ─── GIỮ NGUYÊN CÁC HÀM MẶC ĐỊNH DƯỚI ĐÂY CỦA BẠN ───
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}