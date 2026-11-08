using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    public class SportsController : Controller
    {
        private readonly VarsityDbContext _context;

        public SportsController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: Sports
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Sports
                .Include(s => s.Matches)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.SportName.ToLower().Contains(search.ToLower()));
                ViewData["CurrentSearch"] = search;
            }

            var sports = await query.ToListAsync();
            return View(sports);
        }

        // GET: Sports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sport = await _context.Sports
                .Include(s => s.Matches)
                    .ThenInclude(m => m.HomeFaculty)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.AwayFaculty)
                .Include(s => s.Matches)
                    .ThenInclude(m => m.Venue)
                .FirstOrDefaultAsync(m => m.SportId == id);

            if (sport == null) return NotFound();

            return View(sport);
        }

        // GET: Sports/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Sport());
        }

        // POST: Sports/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("SportName,ScoringType,MaxPeriods")] Sport sport)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sport);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Sport '{sport.SportName}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(sport);
        }

        // GET: Sports/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var sport = await _context.Sports.FindAsync(id);
            if (sport == null) return NotFound();

            return View(sport);
        }

        // POST: Sports/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("SportId,SportName,ScoringType,MaxPeriods")] Sport sport)
        {
            if (id != sport.SportId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sport);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Sport rules for '{sport.SportName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Sports.Any(e => e.SportId == sport.SportId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(sport);
        }

        // GET: Sports/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var sport = await _context.Sports
                .Include(s => s.Matches)
                .FirstOrDefaultAsync(m => m.SportId == id);

            if (sport == null) return NotFound();

            return View(sport);
        }

        // POST: Sports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sport = await _context.Sports
                .Include(s => s.Matches)
                .FirstOrDefaultAsync(s => s.SportId == id);

            if (sport != null)
            {
                if (sport.Matches.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete sport '{sport.SportName}' because it has {sport.Matches.Count} match fixtures linked to it.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Sports.Remove(sport);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Sport '{sport.SportName}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

