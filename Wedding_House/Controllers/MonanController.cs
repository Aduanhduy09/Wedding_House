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

        // GET: Monan
        public async Task<IActionResult> Index()
        {
            return View(await _context.Monans.ToListAsync());
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        private bool MonanExists(int id)
        {
            return _context.Monans.Any(e => e.MaMonAn == id);
        }
    }
}
