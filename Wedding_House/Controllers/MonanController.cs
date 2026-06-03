using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    public class MonanController : Controller
    {
        private readonly WeddingHouseContext _context;

        public MonanController(WeddingHouseContext context)
        {
            _context = context;
        }

        // =======================================================
        // 📋 PHẦN 1: CÁC ACTION MVC TRUYỀN THỐNG (GIỮ NGUYÊN CỦA BẠN)
        // =======================================================

        // GET: Monan
        public async Task<IActionResult> Index()
        {
            // 🌟 ĐÃ SỬA TẠI ĐÂY: Thay đổi từ OrderBy(m => m.TenMonAn) sang OrderBy(m => m.MaMonAn)
            // Giúp danh mục món ăn sắp xếp ngay ngắn theo thứ tự tăng dần số ID: #MON-1, #MON-2, #MON-3...
            return View(await _context.Monans.OrderBy(m => m.MaMonAn).ToListAsync());
        }

        // GET: Monan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monan = await _context.Monans
                .FirstOrDefaultAsync(m => m.MaMonAn == id);
            if (monan == null)
            {
                return NotFound();
            }

            return View(monan);
        }

        // GET: Monan/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Monan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaMonAn,TenMonAn")] Monan monan)
        {
            if (ModelState.IsValid)
            {
                _context.Add(monan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(monan);
        }

        // GET: Monan/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monan = await _context.Monans.FindAsync(id);
            if (monan == null)
            {
                return NotFound();
            }
            return View(monan);
        }

        // POST: Monan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaMonAn,TenMonAn")] Monan monan)
        {
            if (id != monan.MaMonAn)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monan);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonanExists(monan.MaMonAn))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(monan);
        }

        // GET: Monan/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monan = await _context.Monans
                .FirstOrDefaultAsync(m => m.MaMonAn == id);
            if (monan == null)
            {
                return NotFound();
            }

            return View(monan);
        }

        // POST: Monan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monan = await _context.Monans.FindAsync(id);
            if (monan != null)
            {
                _context.Monans.Remove(monan);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // =======================================================
        // 🌟 PHẦN 2: CÁC API PHỤC VỤ THAO TÁC MODAL VÀ AJAX ĐỘNG (GIỮ NGUYÊN)
        // =======================================================

        // API: Tiếp nhận yêu cầu đổi tên món ăn từ khung Modal, ghi đè trực tiếp xuống DB
        [HttpPost("/api/Monan/update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDish([FromBody] MonanUpdateDto model)
        {
            try
            {
                var monAn = await _context.Monans.FirstOrDefaultAsync(m => m.MaMonAn == model.MaMonAn);
                if (monAn == null)
                    return NotFound(new { message = "Không tìm thấy món ăn này trong hệ thống!" });

                monAn.TenMonAn = model.TenMonAn;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật tên món ăn mới lên hệ thống thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật món ăn: " + ex.Message });
            }
        }

        // API: Xóa thẳng món ăn ra khỏi cơ sở dữ liệu khi Admin bấm nút xác nhận
        [HttpDelete("/api/Monan/delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            try
            {
                var monAn = await _context.Monans.FirstOrDefaultAsync(m => m.MaMonAn == id);
                if (monAn == null)
                    return NotFound(new { message = "Món ăn không tồn tại hoặc đã bị xóa trước đó!" });

                _context.Monans.Remove(monAn);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa món ăn ra khỏi cơ sở dữ liệu thành công!" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Không thể xóa! Món ăn này hiện đang nằm trong danh sách thực đơn tiệc cưới của khách hàng." });
            }
        }

        private bool MonanExists(int id)
        {
            return _context.Monans.Any(e => e.MaMonAn == id);
        }
    }

    public class MonanUpdateDto
    {
        public int MaMonAn { get; set; }
        public string TenMonAn { get; set; }
    }
}