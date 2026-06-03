using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // =======================================================
        // PHÂN HỆ 1: CÁC TUYẾN ĐƯỜNG DÀNH CHO KHÁCH HÀNG (CLIENT)
        // =======================================================

        // 1. Trang chủ chính thức của hệ thống (Đường dẫn: / hoặc /Home/Index)
        public IActionResult Index()
        {
            return View();
        }

        //  2. Trang xem danh sách thực đơn món ăn (Đường dẫn: /Home/ThucDon)
        public IActionResult ThucDon()
        {
            return View(); // Gọi file Views/Home/ThucDon.cshtml
        }

        // 3. Trang đặt tiệc cưới trực tuyến (Đường dẫn: /Home/DatTiec)
        public IActionResult DatTiec()
        {
            return View(); // Gọi file Views/Home/DatTiec.cshtml
        }

        // 4. Trang hiển thị biên lai đơn đặt tiệc (Đường dẫn: /Home/LichSuDatTiec)
        public IActionResult LichSuDatTiec()
        {
            return View(); // Gọi file Views/Home/LichSuDatTiec.cshtml
        }

        // 5. Trang quản lý hồ sơ và lịch sử cá nhân của khách (Đường dẫn: /Home/TaiKhoan)
        public IActionResult TaiKhoan()
        {
            return View(); // Gọi file Views/Home/TaiKhoan.cshtml
        }


        // =======================================================
        // PHÂN HỆ 2: CÁC TUYẾN ĐƯỜNG DÀNH CHO QUẢN TRỊ VIÊN (ADMIN)
        // =======================================================

        // 📊 1. Bảng điều khiển quản trị chính thức (Đường dẫn: /Home/AdminDashboard)
        public IActionResult AdminDashboard()
        {
            return View();
        }

        // 🔀 2. Tuyến đường điều hướng dự phòng cho hệ thống Admin
        public IActionResult Dashboard()
        {
            return View("AdminDashboard");
        }

        // 🏢 3. 🌟 ĐÃ SỬA TẠI ĐÂY: Hứng chính xác URL từ nút bấm Sidebar truyền sang
        [HttpGet("/SanhTiec/Index")]
        public IActionResult QuanLySanhTiec()
        {
            // Ép Server trả về đúng file giao diện danh sách đơn đã cọc của Admin
            return View("QuanLySanhTiec");
        }
    }
}