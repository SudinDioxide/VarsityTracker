using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    public class FacultiesController : Controller
    {
        private readonly VarsityDbContext _context;

        public FacultiesController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: Faculties
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Faculties
                .Include(f => f.Players)
                .Include(f => f.FacultyUsers)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f => f.FacultyCode.ToLower().Contains(search.ToLower()) 
                                      || f.FacultyName.ToLower().Contains(search.ToLower())
                                      || f.ManagerName.ToLower().Contains(search.ToLower()));
                ViewData["CurrentSearch"] = search;
            }

            var faculties = await query.ToListAsync();
            return View(faculties);
        }

        // GET: Faculties/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var faculty = await _context.Faculties
                .Include(f => f.Players)
                .Include(f => f.FacultyUsers)
                .FirstOrDefaultAsync(m => m.FacultyId == id);

            if (faculty == null) return NotFound();

            // Load home and away matches
            ViewBag.HomeMatches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Where(m => m.HomeFacultyId == id)
                .ToListAsync();

            ViewBag.AwayMatches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.Venue)
                .Where(m => m.AwayFacultyId == id)
                .ToListAsync();

            return View(faculty);
        }

        // GET: Faculties/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Faculty());
        }

        // POST: Faculties/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FacultyCode,FacultyName,ManagerName")] Faculty faculty)
        {
            if (await _context.Faculties.AnyAsync(f => f.FacultyCode.ToLower() == faculty.FacultyCode.ToLower()))
            {
                ModelState.AddModelError("FacultyCode", "A faculty with this code already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(faculty);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Faculty '{faculty.FacultyCode}' registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(faculty);
        }

        // GET: Faculties/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null) return NotFound();

            return View(faculty);
        }

        // POST: Faculties/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("FacultyId,FacultyCode,FacultyName,ManagerName")] Faculty faculty)
        {
            if (id != faculty.FacultyId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(faculty);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Faculty '{faculty.FacultyCode}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Faculties.Any(e => e.FacultyId == faculty.FacultyId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(faculty);
        }

        // GET: Faculties/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var faculty = await _context.Faculties
                .Include(f => f.Players)
                .FirstOrDefaultAsync(m => m.FacultyId == id);

            if (faculty == null) return NotFound();

            return View(faculty);
        }

        // POST: Faculties/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var faculty = await _context.Faculties
                .Include(f => f.Players)
                .FirstOrDefaultAsync(f => f.FacultyId == id);

            if (faculty != null)
            {
                bool hasMatches = await _context.Matches.AnyAsync(m => m.HomeFacultyId == id || m.AwayFacultyId == id);
                if (hasMatches)
                {
                    TempData["ErrorMessage"] = $"Cannot delete faculty '{faculty.FacultyCode}' because it has scheduled match fixtures.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Faculties.Remove(faculty);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Faculty '{faculty.FacultyCode}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

