using Microsoft.AspNetCore.Mvc;

namespace Wedding_House.Controllers
{
    public class TaikhoanController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult XacThucGmail()
        {
            return View();
        }
    }
}