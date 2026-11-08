using System;
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
    public class MatchesController : Controller
    {
        private readonly VarsityDbContext _context;

        public MatchesController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: Matches
        public async Task<IActionResult> Index(string? status, int? sportId, string? search)
        {
            var query = _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Include(m => m.Scorekeeper)
                .Include(m => m.MatchScores)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(m => m.Status.ToLower() == status.ToLower());
                ViewData["CurrentStatus"] = status;
            }

            if (sportId.HasValue && sportId.Value > 0)
            {
                query = query.Where(m => m.SportId == sportId.Value);
                ViewData["CurrentSportId"] = sportId.Value;
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.HomeFaculty!.FacultyCode.ToLower().Contains(search.ToLower())
                                      || m.AwayFaculty!.FacultyCode.ToLower().Contains(search.ToLower())
                                      || m.Sport!.SportName.ToLower().Contains(search.ToLower())
                                      || m.Venue!.VenueName.ToLower().Contains(search.ToLower()));
                ViewData["CurrentSearch"] = search;
            }

            ViewBag.Sports = new SelectList(await _context.Sports.ToListAsync(), "SportId", "SportName", sportId);

            var matches = await query.OrderByDescending(m => m.Status == "Live")
                                     .ThenBy(m => m.MatchDate)
                                     .ToListAsync();
            return View(matches);
        }

        // GET: Matches/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            var matches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Include(m => m.Scorekeeper)
                .OrderBy(m => m.MatchDate)
                .ToListAsync();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("MatchId,Sport,HomeFaculty,AwayFaculty,HomeScore,AwayScore,Status,Venue,MatchDate,Scorekeeper");

            foreach (var m in matches)
            {
                sb.AppendLine($"{m.MatchId},\"{m.Sport?.SportName}\",\"{m.HomeFaculty?.FacultyCode}\",\"{m.AwayFaculty?.FacultyCode}\",{m.HomeTotalScore},{m.AwayTotalScore},\"{m.Status}\",\"{m.Venue?.VenueName}\",\"{m.MatchDate:yyyy-MM-dd HH:mm}\",\"{m.Scorekeeper?.FullName}\"");
            }

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"UTHM_Varsity_Fixtures_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: Matches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                    .ThenInclude(f => f!.Players)
                .Include(m => m.AwayFaculty)
                    .ThenInclude(f => f!.Players)
                .Include(m => m.Venue)
                .Include(m => m.Scorekeeper)
                .Include(m => m.MatchScores)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            return View(match);
        }

        // GET: Matches/ScoreSheet/5 - Official Institutional Match Score Sheet (Printable Borang Rasmi)
        public async Task<IActionResult> ScoreSheet(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                    .ThenInclude(f => f!.Players)
                .Include(m => m.AwayFaculty)
                    .ThenInclude(f => f!.Players)
                .Include(m => m.Venue)
                .Include(m => m.Scorekeeper)
                .Include(m => m.MatchScores)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            return View(match);
        }

        // GET: Matches/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View(new Match { MatchDate = DateTime.Now.AddDays(1) });
        }

        // POST: Matches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("SportId,HomeFacultyId,AwayFacultyId,VenueId,ScorekeeperId,MatchDate,Status")] Match match)
        {
            if (match.HomeFacultyId == match.AwayFacultyId)
            {
                ModelState.AddModelError("AwayFacultyId", "Home and Away faculties must be different teams.");
            }

            // Verify venue is available
            var venue = await _context.Venues.FindAsync(match.VenueId);
            if (venue != null && !venue.IsAvailable)
            {
                ModelState.AddModelError("VenueId", "The selected venue is currently marked for maintenance/unavailable.");
            }

            if (ModelState.IsValid)
            {
                match.Status = "Scheduled";
                match.HomeTotalScore = 0;
                match.AwayTotalScore = 0;
                _context.Add(match);
                await _context.SaveChangesAsync();

                // Create initial period score record (Period 1)
                _context.MatchScores.Add(new MatchScore
                {
                    MatchId = match.MatchId,
                    PeriodNumber = 1,
                    HomePoints = 0,
                    AwayPoints = 0
                });
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Match fixture scheduled successfully.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(match.SportId, match.HomeFacultyId, match.AwayFacultyId, match.VenueId, match.ScorekeeperId);
            return View(match);
        }

        // GET: Matches/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches.FindAsync(id);
            if (match == null) return NotFound();

            await PopulateDropdowns(match.SportId, match.HomeFacultyId, match.AwayFacultyId, match.VenueId, match.ScorekeeperId);
            return View(match);
        }

        // POST: Matches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("MatchId,SportId,HomeFacultyId,AwayFacultyId,VenueId,ScorekeeperId,MatchDate,Status,HomeTotalScore,AwayTotalScore")] Match match)
        {
            if (id != match.MatchId) return NotFound();

            if (match.HomeFacultyId == match.AwayFacultyId)
            {
                ModelState.AddModelError("AwayFacultyId", "Home and Away faculties must be different teams.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(match);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Match fixture updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Matches.Any(e => e.MatchId == match.MatchId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdowns(match.SportId, match.HomeFacultyId, match.AwayFacultyId, match.VenueId, match.ScorekeeperId);
            return View(match);
        }

        // GET: Matches/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var match = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            return View(match);
        }

        // POST: Matches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match != null)
            {
                _context.Matches.Remove(match);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Match fixture cancelled and removed.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? sportId = null, int? homeId = null, int? awayId = null, int? venueId = null, int? scorekeeperId = null)
        {
            ViewBag.Sports = new SelectList(await _context.Sports.ToListAsync(), "SportId", "SportName", sportId);
            ViewBag.Faculties = new SelectList(await _context.Faculties.ToListAsync(), "FacultyId", "FacultyCode", homeId);
            ViewBag.Venues = new SelectList(await _context.Venues.Where(v => v.IsAvailable).ToListAsync(), "VenueId", "VenueName", venueId);
            ViewBag.Scorekeepers = new SelectList(await _context.Users.Where(u => u.Role == "Scorekeeper" || u.Role == "Admin").ToListAsync(), "UserId", "FullName", scorekeeperId);
        }
    }
}

