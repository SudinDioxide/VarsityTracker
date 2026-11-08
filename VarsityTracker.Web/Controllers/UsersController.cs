using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly VarsityDbContext _context;

        public UsersController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(string? role, string? search)
        {
            var query = _context.Users
                .Include(u => u.Faculty)
                .Include(u => u.AssignedMatches)
                .Include(u => u.MatchReminders)
                .Include(u => u.VenueFeedbacks)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
                ViewData["CurrentRole"] = role;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.FullName.ToLower().Contains(search.ToLower()) 
                                      || u.MatricNo.ToLower().Contains(search.ToLower())
                                      || u.Email.ToLower().Contains(search.ToLower()));
                ViewData["CurrentSearch"] = search;
            }

            var users = await query.OrderBy(u => u.Role).ThenBy(u => u.FullName).ToListAsync();
            return View(users);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Faculty)
                .Include(u => u.AssignedMatches)
                    .ThenInclude(m => m.Sport)
                .Include(u => u.AssignedMatches)
                    .ThenInclude(m => m.HomeFaculty)
                .Include(u => u.AssignedMatches)
                    .ThenInclude(m => m.AwayFaculty)
                .Include(u => u.MatchReminders)
                    .ThenInclude(r => r.Match)
                .Include(u => u.VenueFeedbacks)
                    .ThenInclude(f => f.Venue)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // GET: Users/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyCode");
            return View(new User { Role = "Scorekeeper" });
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MatricNo,FullName,Email,PasswordHash,Role,FacultyId")] User user)
        {
            if (await _context.Users.AnyAsync(u => u.MatricNo.ToLower() == user.MatricNo.ToLower()))
            {
                ModelState.AddModelError("MatricNo", "A user with this Matric / Staff ID already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User '{user.FullName}' ({user.Role}) registered successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyCode", user.FacultyId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyCode", user.FacultyId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,MatricNo,FullName,Email,PasswordHash,Role,FacultyId")] User user)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"User '{user.FullName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == user.UserId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyCode", user.FacultyId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Faculty)
                .Include(u => u.AssignedMatches)
                .FirstOrDefaultAsync(m => m.UserId == id);

            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User '{user.FullName}' account deleted.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

