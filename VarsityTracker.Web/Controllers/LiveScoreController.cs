using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    public class LiveScoreController : Controller
    {
        private readonly VarsityDbContext _context;

        public LiveScoreController(VarsityDbContext context)
        {
            _context = context;
        }

        // GET: /LiveScore
        public async Task<IActionResult> Index()
        {
            string? currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? currentUserId = int.TryParse(currentUserIdStr, out int uid) ? uid : null;
            bool isScorekeeper = User.IsInRole("Scorekeeper");

            var query = _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Include(m => m.Scorekeeper)
                .Include(m => m.MatchScores)
                .AsQueryable();

            if (isScorekeeper && currentUserId.HasValue && !User.IsInRole("Admin"))
            {
                // Scorekeeper sees their assigned matches prioritized
                query = query.Where(m => m.ScorekeeperId == currentUserId.Value || m.Status == "Live");
            }

            var matches = await query.OrderByDescending(m => m.Status == "Live")
                                     .ThenBy(m => m.MatchDate)
                                     .ToListAsync();

            return View(matches);
        }

        // GET: /LiveScore/Console/5
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> Console(int? id)
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
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            // Ensure at least Period 1 exists
            if (!match.MatchScores.Any())
            {
                var p1 = new MatchScore
                {
                    MatchId = match.MatchId,
                    PeriodNumber = 1,
                    HomePoints = match.HomeTotalScore,
                    AwayPoints = match.AwayTotalScore
                };
                _context.MatchScores.Add(p1);
                await _context.SaveChangesAsync();
                match.MatchScores.Add(p1);
            }

            return View(match);
        }

        // AJAX API: Update Score
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> UpdateScore([FromBody] UpdateScoreRequest request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request payload." });

            var match = await _context.Matches
                .Include(m => m.MatchScores)
                .FirstOrDefaultAsync(m => m.MatchId == request.MatchId);

            if (match == null) return NotFound(new { success = false, message = "Match not found." });

            // If match is not Live, automatically start it if incrementing
            if (match.Status == "Scheduled" && (request.HomeDelta > 0 || request.AwayDelta > 0))
            {
                match.Status = "Live";
            }

            // Target period score
            var currentPeriod = match.MatchScores.FirstOrDefault(ms => ms.PeriodNumber == request.PeriodNumber);
            if (currentPeriod == null)
            {
                currentPeriod = new MatchScore
                {
                    MatchId = match.MatchId,
                    PeriodNumber = request.PeriodNumber,
                    HomePoints = 0,
                    AwayPoints = 0
                };
                _context.MatchScores.Add(currentPeriod);
            }

            currentPeriod.HomePoints = Math.Max(0, currentPeriod.HomePoints + request.HomeDelta);
            currentPeriod.AwayPoints = Math.Max(0, currentPeriod.AwayPoints + request.AwayDelta);

            // Recompute total scores across all periods
            match.HomeTotalScore = match.MatchScores.Sum(ms => ms.HomePoints);
            match.AwayTotalScore = match.MatchScores.Sum(ms => ms.AwayPoints);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                matchId = match.MatchId,
                status = match.Status,
                homeTotalScore = match.HomeTotalScore,
                awayTotalScore = match.AwayTotalScore,
                periodNumber = currentPeriod.PeriodNumber,
                periodHomePoints = currentPeriod.HomePoints,
                periodAwayPoints = currentPeriod.AwayPoints,
                periods = match.MatchScores.OrderBy(p => p.PeriodNumber).Select(p => new
                {
                    periodNumber = p.PeriodNumber,
                    homePoints = p.HomePoints,
                    awayPoints = p.AwayPoints
                })
            });
        }

        // AJAX API: Change Status (Scheduled -> Live -> Completed / Cancelled)
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> ChangeStatus([FromBody] ChangeStatusRequest request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request." });

            var match = await _context.Matches.FindAsync(request.MatchId);
            if (match == null) return NotFound(new { success = false, message = "Match not found." });

            match.Status = request.NewStatus;
            await _context.SaveChangesAsync();

            return Json(new { success = true, matchId = match.MatchId, status = match.Status });
        }

        // AJAX API: Add New Period / Set
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> AddPeriod([FromBody] AddPeriodRequest request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request." });

            var match = await _context.Matches
                .Include(m => m.MatchScores)
                .Include(m => m.Sport)
                .FirstOrDefaultAsync(m => m.MatchId == request.MatchId);

            if (match == null) return NotFound(new { success = false, message = "Match not found." });

            int maxPeriodAllowed = match.Sport?.MaxPeriods ?? 10;
            int nextPeriod = (match.MatchScores.Any() ? match.MatchScores.Max(p => p.PeriodNumber) : 0) + 1;

            if (nextPeriod > maxPeriodAllowed)
            {
                return BadRequest(new { success = false, message = $"Maximum allowed periods ({maxPeriodAllowed}) reached for {match.Sport?.SportName}." });
            }

            var newPeriod = new MatchScore
            {
                MatchId = match.MatchId,
                PeriodNumber = nextPeriod,
                HomePoints = 0,
                AwayPoints = 0
            };
            _context.MatchScores.Add(newPeriod);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                periodNumber = newPeriod.PeriodNumber,
                homePoints = newPeriod.HomePoints,
                awayPoints = newPeriod.AwayPoints
            });
        }

        // Real-Time Live Feed Endpoint for polling (used by Live Score ticker and spectator view)
        [HttpGet]
        public async Task<IActionResult> GetLiveFeed(int id)
        {
            var match = await _context.Matches
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Sport)
                .Include(m => m.Venue)
                .Include(m => m.MatchScores)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (match == null) return NotFound();

            return Json(new
            {
                matchId = match.MatchId,
                status = match.Status,
                homeFaculty = match.HomeFaculty?.FacultyCode,
                awayFaculty = match.AwayFaculty?.FacultyCode,
                homeTotalScore = match.HomeTotalScore,
                awayTotalScore = match.AwayTotalScore,
                sport = match.Sport?.SportName,
                scoringType = match.Sport?.ScoringType,
                venue = match.Venue?.VenueName,
                periods = match.MatchScores.OrderBy(p => p.PeriodNumber).Select(p => new
                {
                    period = p.PeriodNumber,
                    home = p.HomePoints,
                    away = p.AwayPoints
                }),
                events = await _context.MatchEvents
                    .Where(e => e.MatchId == id)
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(10)
                    .Select(e => new
                    {
                        eventId = e.MatchEventId,
                        eventType = e.EventType,
                        minute = e.Minute,
                        description = e.Description,
                        playerName = e.PlayerName,
                        facultyId = e.FacultyId,
                        createdAt = e.CreatedAt.ToString("HH:mm:ss")
                    })
                    .ToListAsync()
            });
        }

        // AJAX API: Add Match Event / Commentary
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> AddEvent([FromBody] AddMatchEventRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Description))
            {
                return BadRequest(new { success = false, message = "Event description is required." });
            }

            var match = await _context.Matches.FindAsync(request.MatchId);
            if (match == null) return NotFound(new { success = false, message = "Match not found." });

            var ev = new MatchEvent
            {
                MatchId = request.MatchId,
                FacultyId = request.FacultyId,
                EventType = request.EventType,
                Minute = string.IsNullOrWhiteSpace(request.Minute) ? $"{DateTime.Now:mm}'" : request.Minute,
                Description = request.Description,
                PlayerName = request.PlayerName,
                CreatedAt = DateTime.Now
            };

            _context.MatchEvents.Add(ev);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                eventId = ev.MatchEventId,
                eventType = ev.EventType,
                minute = ev.Minute,
                description = ev.Description,
                playerName = ev.PlayerName,
                facultyId = ev.FacultyId,
                createdAt = ev.CreatedAt.ToString("HH:mm:ss")
            });
        }

        // AJAX API: Delete Match Event
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var ev = await _context.MatchEvents.FindAsync(id);
            if (ev == null) return NotFound(new { success = false, message = "Event not found." });

            _context.MatchEvents.Remove(ev);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // AJAX API: Get All Events for a Match
        [HttpGet]
        public async Task<IActionResult> GetEvents(int matchId)
        {
            var events = await _context.MatchEvents
                .Include(e => e.Faculty)
                .Where(e => e.MatchId == matchId)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    eventId = e.MatchEventId,
                    eventType = e.EventType,
                    minute = e.Minute,
                    description = e.Description,
                    playerName = e.PlayerName,
                    facultyCode = e.Faculty != null ? e.Faculty.FacultyCode : "",
                    createdAt = e.CreatedAt.ToString("HH:mm:ss")
                })
                .ToListAsync();

            return Json(events);
        }

        // FEATURE 5: 1-Click Live Match Demo Simulator
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> SimulateDemoPulse([FromBody] DemoPulseRequest request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request." });

            var match = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty).ThenInclude(f => f!.Players)
                .Include(m => m.AwayFaculty).ThenInclude(f => f!.Players)
                .Include(m => m.MatchScores)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.MatchId == request.MatchId);

            if (match == null) return NotFound(new { success = false, message = "Match not found." });

            if (match.Status != "Live")
            {
                match.Status = "Live";
            }

            var activePeriod = match.MatchScores.OrderByDescending(p => p.PeriodNumber).FirstOrDefault();
            if (activePeriod == null)
            {
                activePeriod = new MatchScore { MatchId = match.MatchId, PeriodNumber = 1, HomePoints = 0, AwayPoints = 0 };
                _context.MatchScores.Add(activePeriod);
            }

            bool homeScores = match.HomeTotalScore <= match.AwayTotalScore;
            var scoringFaculty = homeScores ? match.HomeFaculty : match.AwayFaculty;
            int delta = 1;
            string sportName = match.Sport?.SportName ?? "Match";

            if (sportName.Contains("Basketball", StringComparison.OrdinalIgnoreCase))
            {
                delta = Random.Shared.Next(1, 10) > 4 ? 2 : 3;
            }

            if (homeScores)
            {
                activePeriod.HomePoints += delta;
            }
            else
            {
                activePeriod.AwayPoints += delta;
            }

            match.HomeTotalScore = match.MatchScores.Sum(ms => ms.HomePoints);
            match.AwayTotalScore = match.MatchScores.Sum(ms => ms.AwayPoints);

            var squad = scoringFaculty?.Players?.ToList();
            string? shooter = squad != null && squad.Any() 
                ? squad[Random.Shared.Next(squad.Count)].FullName 
                : (homeScores ? "Striker #10" : "Forward #7");

            int simulatedMin = Math.Min(90, (match.Events.Count + 1) * 7);
            string minuteStr = $"{simulatedMin}'";

            string verb = sportName.Contains("Basketball", StringComparison.OrdinalIgnoreCase) 
                ? $"{delta}-point shot scored" 
                : (sportName.Contains("Badminton", StringComparison.OrdinalIgnoreCase) ? "Smash winner point" : "Goal scored!");

            var newEvent = new MatchEvent
            {
                MatchId = match.MatchId,
                FacultyId = scoringFaculty?.FacultyId,
                EventType = sportName.Contains("Basketball", StringComparison.OrdinalIgnoreCase) ? "Point" : "Goal",
                Minute = minuteStr,
                Description = $"{verb} by {shooter} ({scoringFaculty?.FacultyCode})",
                PlayerName = shooter,
                CreatedAt = DateTime.Now
            };
            _context.MatchEvents.Add(newEvent);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                matchId = match.MatchId,
                status = match.Status,
                homeTotalScore = match.HomeTotalScore,
                awayTotalScore = match.AwayTotalScore,
                lastScorer = shooter,
                lastFaculty = scoringFaculty?.FacultyCode,
                lastDescription = newEvent.Description,
                minute = minuteStr
            });
        }

        // Reset Demo Match back to Scheduled 0-0 for re-runs
        [HttpPost]
        [Authorize(Roles = "Admin,Scorekeeper")]
        public async Task<IActionResult> ResetDemoMatch([FromBody] DemoPulseRequest request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request." });

            var match = await _context.Matches
                .Include(m => m.MatchScores)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.MatchId == request.MatchId);

            if (match == null) return NotFound(new { success = false, message = "Match not found." });

            match.Status = "Scheduled";
            match.HomeTotalScore = 0;
            match.AwayTotalScore = 0;

            foreach (var ms in match.MatchScores)
            {
                ms.HomePoints = 0;
                ms.AwayPoints = 0;
            }

            _context.MatchEvents.RemoveRange(match.Events);

            await _context.SaveChangesAsync();

            return Json(new { success = true, matchId = match.MatchId, status = match.Status });
        }
    }

    public class UpdateScoreRequest
    {
        public int MatchId { get; set; }
        public int PeriodNumber { get; set; } = 1;
        public int HomeDelta { get; set; }
        public int AwayDelta { get; set; }
    }

    public class ChangeStatusRequest
    {
        public int MatchId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }

    public class AddPeriodRequest
    {
        public int MatchId { get; set; }
    }

    public class AddMatchEventRequest
    {
        public int MatchId { get; set; }
        public int? FacultyId { get; set; }
        public string EventType { get; set; } = "Goal";
        public string Minute { get; set; } = "1'";
        public string Description { get; set; } = string.Empty;
        public string? PlayerName { get; set; }
    }

    public class DemoPulseRequest
    {
        public int MatchId { get; set; }
    }
}

