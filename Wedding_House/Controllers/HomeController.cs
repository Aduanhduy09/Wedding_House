using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    public class HomeController : Controller
    {
        // 1. TRANG CHỦ CHO KHÁCH HÀNG (Đường dẫn: / hoặc /Home/Index)
        public IActionResult Index()
        {
            return View();
        }

        // 2. TUYẾN ĐƯỜNG DỰ PHÒNG (Đường dẫn: /Home/Dashboard)
        // Nếu lỡ có chỗ nào gọi link cũ, lệnh này sẽ ép Server tìm đúng file "AdminDashboard" của bạn
        public IActionResult Dashboard()
        {
            return View("AdminDashboard");
        }

        // 3. TUYẾN ĐƯỜNG CHÍNH ĐỒNG BỘ (Đường dẫn: /Home/AdminDashboard)
        // Hàm này khớp hoàn toàn với tên file AdminDashboard.cshtml bạn đã tạo
        public IActionResult AdminDashboard()
        {
            return View(); // Tự động tìm file AdminDashboard.cshtml cùng tên với hàm
        }

        // 4. TUYẾN ĐƯỜNG XEM TOÀN BỘ THỰC ĐƠN MỚI BỔ SUNG (Đường dẫn: /Home/ThucDon)
        // Hàm này chịu trách nhiệm gọi và hiển thị file ThucDon.cshtml chứa 101 món ăn
        public IActionResult ThucDon()
        {
            return View(); // Tự động tìm file ThucDon.cshtml trong thư mục Views/Home
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        //  TUYẾN ĐƯỜNG XEM CHI TIẾT SẢNH TIỆC (Đường dẫn: /Home/SanhTiec)
        public IActionResult SanhTiec()
        {
            return View(); // Sẽ gọi file Views/Home/SanhTiec.cshtml
        }
        // TUYẾN ĐƯỜNG ĐẶT TIỆC CƯỚI TRỰC TUYẾN (Đường dẫn: /Home/DatTiec)
        public IActionResult DatTiec()
        {
            return View(); // Sẽ gọi file Views/Home/DatTiec.cshtml
        }
        // 🧾 TUYẾN ĐƯỜNG XEM BIÊN LAI ĐƠN ĐẶT TIỆC (Đường dẫn: /Home/BiênLai hoặc /Home/LichSuDatTiec)
        public IActionResult LichSuDatTiec()
        {
            return View(); // Sẽ gọi file Views/Home/LichSuDatTiec.cshtml
        }
        public IActionResult TaiKhoan()
        {
            // Chuyển hướng đến trang TaiKhoan.cshtml
            return View();
        }
    }
}