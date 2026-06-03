using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    // 🌟 ĐÃ SỬA: Bỏ Authorize ở tầng class để trình duyệt có thể tải được giao diện HTML của trang Index
    public class ThamsoController : Controller
    {
        private readonly WeddingHouseContext _context;

        public ThamsoController(WeddingHouseContext context)
        {
            _context = context;
        }

        // GET: /Thamso/Index
        [HttpGet("/ThamSo/Index")]
        public async Task<IActionResult> Index()
        {
            await Task.CompletedTask; // keep method asynchronous for hot-reload safety
            return View();
        }

        // =======================================================
        // 🌟 API 1: LẤY TOÀN BỘ CÁC DÒNG QUY ĐỊNH TỪ BẢNG THAMSO
        // =======================================================
        [HttpGet("/api/Thamso/get-global-params")]
        public async Task<IActionResult> GetGlobalParams()
        {
            try
            {
                // Bóc trực tiếp từ bảng Thamsos theo đúng sơ đồ ERD
                var danhSachThamSo = await _context.Thamsos.ToListAsync();
                return Ok(danhSachThamSo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy bảng tham số: " + ex.Message });
            }
        }

        // =======================================================
        // 🌟 API 2: CẬP NHẬT GIÁ TRỊ THAM SỐ XUỐNG BẢNG THAMSO
        // =======================================================
        [HttpPost("/api/Thamso/update-global-param")]
        public async Task<IActionResult> UpdateGlobalParam([FromBody] Thamso model)
        {
            try
            {
                var thamSoGoc = await _context.Thamsos.FirstOrDefaultAsync(t => t.Id == model.Id);
                if (thamSoGoc == null) return NotFound(new { message = "Không tìm thấy dòng tham số cần cập nhật!" });

                // Cập nhật các trường dữ liệu trực thuộc bảng THAMSO
                thamSoGoc.SoLuongBanToiDa = model.SoLuongBanToiDa;
                thamSoGoc.DonGiaBanToiThieu = model.DonGiaBanToiThieu;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật tham số quy định hệ thống thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi lưu tham số: " + ex.Message });
            }
        }

        // Giữ lại các hàm Edit gốc bằng form truyền thống của bạn đề phòng dùng đến
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var thamso = await _context.Thamsos.FindAsync(id);
            if (thamso == null) return NotFound();
            return View(thamso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Thamso thamso)
        {
            if (id != thamso.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thamso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Thamsos.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(thamso);
        }
    }
}