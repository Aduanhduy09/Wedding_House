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
    public class CaController : Controller
    {
        private readonly WeddingHouseContext _context;

        public CaController(WeddingHouseContext context)
        {
            _context = context;
        }

        // GET: Ca
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cas.ToListAsync());
        }

        // GET: Ca/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ca = await _context.Cas
                .FirstOrDefaultAsync(m => m.MaCa == id);
            if (ca == null)
            {
                return NotFound();
            }

            return View(ca);
        }

        // GET: Ca/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Ca/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCa,TenCa,Gio")] Ca ca)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ca);
        }

        // GET: Ca/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ca = await _context.Cas.FindAsync(id);
            if (ca == null)
            {
                return NotFound();
            }
            return View(ca);
        }

        // POST: Ca/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaCa,TenCa,Gio")] Ca ca)
        {
            if (id != ca.MaCa)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ca);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaExists(ca.MaCa))
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
            return View(ca);
        }

        // GET: Ca/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ca = await _context.Cas
                .FirstOrDefaultAsync(m => m.MaCa == id);
            if (ca == null)
            {
                return NotFound();
            }

            return View(ca);
        }

        // POST: Ca/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ca = await _context.Cas.FindAsync(id);
            if (ca != null)
            {
                _context.Cas.Remove(ca);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CaExists(int id)
        {
            return _context.Cas.Any(e => e.MaCa == id);
        }
    }
}
