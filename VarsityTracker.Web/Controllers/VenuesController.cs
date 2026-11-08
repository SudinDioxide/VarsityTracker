using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    public class VenuesController : Controller
    {
        private readonly VarsityDbContext _context;
        private readonly IWebHostEnvironment _env;

        public VenuesController(VarsityDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Venues
        public async Task<IActionResult> Index(string? status, string? search)
        {
            var query = _context.Venues
                .Include(v => v.Matches)
                .Include(v => v.VenueFeedbacks)
                .AsQueryable();

            if (status == "available")
            {
                query = query.Where(v => v.IsAvailable);
                ViewData["CurrentStatus"] = "available";
            }
            else if (status == "maintenance")
            {
                query = query.Where(v => !v.IsAvailable);
                ViewData["CurrentStatus"] = "maintenance";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v => v.VenueName.ToLower().Contains(search.ToLower()) 
                                      || v.Location.ToLower().Contains(search.ToLower()));
                ViewData["CurrentSearch"] = search;
            }

            var venues = await query.ToListAsync();
            return View(venues);
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .Include(v => v.Matches)
                    .ThenInclude(m => m.Sport)
                .Include(v => v.Matches)
                    .ThenInclude(m => m.HomeFaculty)
                .Include(v => v.Matches)
                    .ThenInclude(m => m.AwayFaculty)
                .Include(v => v.VenueFeedbacks)
                    .ThenInclude(f => f.User)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            return View(venue);
        }

        // GET: Venues/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new Venue { IsAvailable = true });
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("VenueName,Location,IsAvailable,ImageUrl")] Venue venue, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "venues");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"venue_{Guid.NewGuid():N}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                venue.ImageUrl = $"/uploads/venues/{uniqueFileName}";
            }

            if (ModelState.IsValid)
            {
                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' registered successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // GET: Venues/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("VenueId,VenueName,Location,IsAvailable,ImageUrl")] Venue venue, IFormFile? imageFile)
        {
            if (id != venue.VenueId) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "venues");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = $"venue_{Guid.NewGuid():N}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                venue.ImageUrl = $"/uploads/venues/{uniqueFileName}";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Venues.Any(e => e.VenueId == venue.VenueId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        // Quick Toggle IsAvailable (Safety Maintenance Action)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleAvailability(int id, string? returnUrl = null)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                venue.IsAvailable = !venue.IsAvailable;
                await _context.SaveChangesAsync();

                string statusText = venue.IsAvailable ? "AVAILABLE for scheduling" : "CLOSED FOR MAINTENANCE";
                TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' status updated to {statusText}.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Venues/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues
                .Include(v => v.Matches)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null) return NotFound();

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Matches)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue != null)
            {
                if (venue.Matches.Any())
                {
                    TempData["ErrorMessage"] = $"Cannot delete venue '{venue.VenueName}' because it has {venue.Matches.Count} match fixtures scheduled.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

