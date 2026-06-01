using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wedding_House.Models;

namespace Wedding_House.Controllers
{
    public class ThamsoController : Controller
    {
        private readonly WeddingHouseContext _context;

        public ThamsoController(WeddingHouseContext context)
        {
            _context = context;
        }

        // GET: Thamso
        // Hiển thị toàn bộ các quy định/tham số hệ thống
        public async Task<IActionResult> Index()
        {
            var danhSachThamSo = await _context.Thamsos.ToListAsync();
            return View(danhSachThamSo);
        }

        // GET: Thamso/Edit/5
        // Mở form thay đổi giá trị của một tham số
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var thamso = await _context.Thamsos.FindAsync(id);
            if (thamso == null)
            {
                return NotFound();
            }
            return View(thamso);
        }

        // POST: Thamso/Edit/5
        // Xử lý lưu giá trị tham số mới vào Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Thamso thamso)
        {
            
            if (id != thamso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(thamso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ThamsoExists(thamso.Id))
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
            return View(thamso);
        }

        private bool ThamsoExists(int id)
        {
            return _context.Thamsos.Any(e => e.Id == id);
        }
    }
}