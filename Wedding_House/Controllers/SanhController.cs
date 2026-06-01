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
    public class SanhController : Controller
    {
        private readonly WeddingHouseContext _context;

        public SanhController(WeddingHouseContext context)
        {
            _context = context;
        }

        // GET: Sanh
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sanhs.ToListAsync());
        }

        // GET: Sanh/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanh = await _context.Sanhs
                .FirstOrDefaultAsync(m => m.MaSanh == id);
            if (sanh == null)
            {
                return NotFound();
            }

            return View(sanh);
        }

        // GET: Sanh/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Sanh/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSanh,TenSanh,LoaiSanh,GhiChu")] Sanh sanh)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sanh);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sanh);
        }

        // GET: Sanh/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanh = await _context.Sanhs.FindAsync(id);
            if (sanh == null)
            {
                return NotFound();
            }
            return View(sanh);
        }

        // POST: Sanh/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaSanh,TenSanh,LoaiSanh,GhiChu")] Sanh sanh)
        {
            if (id != sanh.MaSanh)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanh);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanhExists(sanh.MaSanh))
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
            return View(sanh);
        }

        // GET: Sanh/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanh = await _context.Sanhs
                .FirstOrDefaultAsync(m => m.MaSanh == id);
            if (sanh == null)
            {
                return NotFound();
            }

            return View(sanh);
        }

        // POST: Sanh/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanh = await _context.Sanhs.FindAsync(id);
            if (sanh != null)
            {
                _context.Sanhs.Remove(sanh);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SanhExists(int id)
        {
            return _context.Sanhs.Any(e => e.MaSanh == id);
        }
    }
}
