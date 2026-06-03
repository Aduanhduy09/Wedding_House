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
    public class DichvuController : Controller
    {
        private readonly WeddingHouseContext _context;

        public DichvuController(WeddingHouseContext context)
        {
            _context = context;
        }

        // =======================================================
        // 📋 PHẦN 1: CÁC ACTION MVC TRUYỀN THỐNG (GIỮ NGUYÊN CỦA BẠN)
        // =======================================================

        // GET: Dichvu
        public async Task<IActionResult> Index()
        {
            // 🌟 ĐÃ SỬA TẠI ĐÂY: Thay đổi từ OrderBy(TenDichVu) sang OrderBy(d => d.MaDichVu)
            // Lệnh này giúp sắp xếp danh mục tăng dần từ #SRV-1, #SRV-2, #SRV-3... trở lên
            return View(await _context.Dichvus.OrderBy(d => d.MaDichVu).ToListAsync());
        }

        // GET: Dichvu/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dichvu = await _context.Dichvus
                .FirstOrDefaultAsync(m => m.MaDichVu == id);
            if (dichvu == null)
            {
                return NotFound();
            }

            return View(dichvu);
        }

        // GET: Dichvu/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Dichvu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaDichVu,TenDichVu")] Dichvu dichvu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dichvu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dichvu);
        }

        // GET: Dichvu/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dichvu = await _context.Dichvus.FindAsync(id);
            if (dichvu == null)
            {
                return NotFound();
            }
            return View(dichvu);
        }

        // POST: Dichvu/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDichVu,TenDichVu")] Dichvu dichvu)
        {
            if (id != dichvu.MaDichVu)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dichvu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DichvuExists(dichvu.MaDichVu))
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
            return View(dichvu);
        }

        // GET: Dichvu/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dichvu = await _context.Dichvus
                .FirstOrDefaultAsync(m => m.MaDichVu == id);
            if (dichvu == null)
            {
                return NotFound();
            }

            return View(dichvu);
        }

        // POST: Dichvu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dichvu = await _context.Dichvus.FindAsync(id);
            if (dichvu != null)
            {
                _context.Dichvus.Remove(dichvu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // =======================================================
        // 🌟 PHẦN 2: CÁC API PHỤC VỤ THAO TÁC MODAL VÀ AJAX ĐỘNG
        // =======================================================

        // API: Cập nhật đổi tên dịch vụ từ khung Modal và ghi trực tiếp xuống Database
        [HttpPost("/api/Dichvu/update")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService([FromBody] DichvuUpdateDto model)
        {
            try
            {
                var dichVu = await _context.Dichvus.FirstOrDefaultAsync(d => d.MaDichVu == model.MaDichVu);
                if (dichVu == null)
                    return NotFound(new { message = "Không tìm thấy dịch vụ này trên hệ thống!" });

                dichVu.TenDichVu = model.TenDichVu;

                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật tên dịch vụ mới lên hệ thống thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật dịch vụ: " + ex.Message });
            }
        }

        // API: Xóa thẳng dịch vụ ra khỏi cơ sở dữ liệu khi nhận lệnh từ nút xác nhận
        [HttpDelete("/api/Dichvu/delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var dichVu = await _context.Dichvus.FirstOrDefaultAsync(d => d.MaDichVu == id);
                if (dichVu == null)
                    return NotFound(new { message = "Dịch vụ không tồn tại hoặc đã bị xóa trước đó!" });

                _context.Dichvus.Remove(dichVu);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa dịch vụ ra khỏi cơ sở dữ liệu hệ thống thành công!" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Không thể xóa! Dịch vụ này hiện đang nằm trong danh sách sử dụng tiệc cưới của khách hàng." });
            }
        }


        private bool DichvuExists(int id)
        {
            return _context.Dichvus.Any(e => e.MaDichVu == id);
        }
    }

    public class DichvuUpdateDto
    {
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; }
    }
}