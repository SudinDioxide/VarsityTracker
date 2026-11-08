using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;

namespace VarsityTracker.Web.Controllers
{
    [ApiController]
    [Route("api/mobile")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class MobileApiController : ControllerBase
    {
        private readonly VarsityDbContext _context;

        public MobileApiController(VarsityDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. CHAMPIONSHIP STANDINGS
        // ==========================================
        [HttpGet("standings")]
        public async Task<IActionResult> GetStandings()
        {
            var faculties = await _context.Faculties.ToListAsync();
            var completedMatches = await _context.Matches
                .Where(m => m.Status == "Completed")
                .ToListAsync();

            var list = new List<object>();

            foreach (var f in faculties)
            {
                int played = 0, won = 0, drawn = 0, lost = 0;
                int pointsFor = 0, pointsAgainst = 0, points = 0;

                var homeGames = completedMatches.Where(m => m.HomeFacultyId == f.FacultyId).ToList();
                var awayGames = completedMatches.Where(m => m.AwayFacultyId == f.FacultyId).ToList();
                played = homeGames.Count + awayGames.Count;

                foreach (var g in homeGames)
                {
                    pointsFor += g.HomeTotalScore;
                    pointsAgainst += g.AwayTotalScore;
                    if (g.HomeTotalScore > g.AwayTotalScore) { won++; points += 3; }
                    else if (g.HomeTotalScore == g.AwayTotalScore) { drawn++; points += 1; }
                    else { lost++; }
                }

                foreach (var g in awayGames)
                {
                    pointsFor += g.AwayTotalScore;
                    pointsAgainst += g.HomeTotalScore;
                    if (g.AwayTotalScore > g.HomeTotalScore) { won++; points += 3; }
                    else if (g.AwayTotalScore == g.HomeTotalScore) { drawn++; points += 1; }
                    else { lost++; }
                }

                int pointDiff = pointsFor - pointsAgainst;

                list.Add(new
                {
                    facultyId = f.FacultyId,
                    facultyCode = f.FacultyCode,
                    facultyName = f.FacultyName,
                    played,
                    won,
                    drawn,
                    lost,
                    pointsFor,
                    pointsAgainst,
                    pointDifference = pointDiff,
                    points
                });
            }

            var ordered = list
                .Cast<dynamic>()
                .OrderByDescending(s => (int)s.points)
                .ThenByDescending(s => (int)s.pointDifference)
                .ThenByDescending(s => (int)s.pointsFor)
                .ToList();

            var result = new List<object>();
            int rank = 1;
            foreach (var item in ordered)
            {
                result.Add(new
                {
                    rank = rank++,
                    facultyId = item.facultyId,
                    facultyCode = item.facultyCode,
                    facultyName = item.facultyName,
                    played = item.played,
                    won = item.won,
                    drawn = item.drawn,
                    lost = item.lost,
                    pointsFor = item.pointsFor,
                    pointsAgainst = item.pointsAgainst,
                    pointDifference = item.pointDifference,
                    points = item.points
                });
            }

            return Ok(result);
        }

        // ==========================================
        // 2. MATCHES & LIVE SCORE VIEWER
        // ==========================================
        [HttpGet("matches")]
        public async Task<IActionResult> GetMatches()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var matches = await _context.Matches
                .AsNoTracking()
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Include(m => m.Events)
                .OrderByDescending(m => m.Status == "Live")
                .ThenBy(m => m.MatchDate)
                .ToListAsync();

            var result = matches.Select(m => {
                var lastEv = m.Events?.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
                return new
                {
                    matchId = m.MatchId,
                    sport = m.Sport?.SportName ?? "Sport",
                    scoringType = m.Sport?.ScoringType ?? "Goals",
                    homeFaculty = m.HomeFaculty?.FacultyCode ?? "HOME",
                    awayFaculty = m.AwayFaculty?.FacultyCode ?? "AWAY",
                    homeScore = m.HomeTotalScore,
                    awayScore = m.AwayTotalScore,
                    venue = m.Venue?.VenueName ?? "Campus Venue",
                    matchDate = m.MatchDate,
                    status = m.Status,
                    latestEvent = lastEv != null ? $"{lastEv.Minute} {lastEv.Description}" : string.Empty
                };
            });

            return Ok(result);
        }

        [HttpGet("matches/{id}")]
        public async Task<IActionResult> GetMatchDetails(int id)
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var m = await _context.Matches
                .AsNoTracking()
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Include(m => m.Scorekeeper)
                .Include(m => m.MatchScores)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.MatchId == id);

            if (m == null) return NotFound(new { success = false, message = "Match not found." });

            var homePlayers = await _context.Players
                .Where(p => p.FacultyId == m.HomeFacultyId)
                .Select(p => new
                {
                    playerId = p.PlayerId,
                    fullName = p.FullName,
                    matricNo = p.MatricNo,
                    positionRole = p.PositionRole,
                    photoUrl = p.PhotoUrl
                })
                .ToListAsync();

            var awayPlayers = await _context.Players
                .Where(p => p.FacultyId == m.AwayFacultyId)
                .Select(p => new
                {
                    playerId = p.PlayerId,
                    fullName = p.FullName,
                    matricNo = p.MatricNo,
                    positionRole = p.PositionRole,
                    photoUrl = p.PhotoUrl
                })
                .ToListAsync();

            var result = new
            {
                matchId = m.MatchId,
                sport = m.Sport?.SportName ?? "Sport",
                scoringType = m.Sport?.ScoringType ?? "Goals",
                homeFaculty = m.HomeFaculty?.FacultyCode ?? "HOME",
                homeFacultyName = m.HomeFaculty?.FacultyName ?? "",
                homeManager = m.HomeFaculty?.ManagerName ?? "TBA",
                awayFaculty = m.AwayFaculty?.FacultyCode ?? "AWAY",
                awayFacultyName = m.AwayFaculty?.FacultyName ?? "",
                awayManager = m.AwayFaculty?.ManagerName ?? "TBA",
                homeScore = m.HomeTotalScore,
                awayScore = m.AwayTotalScore,
                venue = m.Venue?.VenueName ?? "Campus Venue",
                venueLocation = m.Venue?.Location ?? "UTHM Campus",
                venueIsAvailable = m.Venue?.IsAvailable ?? true,
                scorekeeper = m.Scorekeeper?.FullName ?? "Official UTHM Referee",
                matchDate = m.MatchDate,
                status = m.Status,
                periods = m.MatchScores.OrderBy(p => p.PeriodNumber).Select(p => new
                {
                    periodNumber = p.PeriodNumber,
                    homePoints = p.HomePoints,
                    awayPoints = p.AwayPoints
                }).ToList(),
                events = m.Events.OrderByDescending(e => e.CreatedAt).Select(e => new
                {
                    matchEventId = e.MatchEventId,
                    eventType = e.EventType,
                    minute = e.Minute,
                    description = e.Description,
                    playerName = e.PlayerName
                }).ToList(),
                homePlayers,
                awayPlayers
            };

            return Ok(result);
        }

        [HttpGet("matches/{id}/events")]
        public async Task<IActionResult> GetMatchEvents(int id)
        {
            var events = await _context.MatchEvents
                .Include(e => e.Faculty)
                .Where(e => e.MatchId == id)
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    matchEventId = e.MatchEventId,
                    eventType = e.EventType,
                    minute = e.Minute,
                    description = e.Description,
                    playerName = e.PlayerName,
                    faculty = e.Faculty != null ? e.Faculty.FacultyCode : ""
                })
                .ToListAsync();

            return Ok(events);
        }

        // ==========================================
        // 3. STUDENT AUTH (Mobile reserved for Students)
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] MobileLoginRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Identifier) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { success = false, message = "Matric number and password are required." });
            }

            var idLower = req.Identifier.Trim().ToLower();
            var user = await _context.Users
                .Include(u => u.Faculty)
                .FirstOrDefaultAsync(u => u.MatricNo.ToLower() == idLower || u.Email.ToLower() == idLower);

            if (user == null || user.PasswordHash != req.Password.Trim())
            {
                return Unauthorized(new { success = false, message = "Invalid student credentials." });
            }

            if (user.Role != "Student")
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = $"Access Restricted: {user.Role} accounts must log in via the Web Management Portal. The Mobile Client is reserved exclusively for Students."
                });
            }

            return Ok(new
            {
                success = true,
                userId = user.UserId,
                matricNo = user.MatricNo,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role,
                facultyId = user.FacultyId,
                facultyCode = user.Faculty?.FacultyCode ?? "FSKTM",
                facultyName = user.Faculty?.FacultyName ?? ""
            });
        }

        // ==========================================
        // 4. STUDENT PROFILE CRUD #1
        // ==========================================
        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfile(int userId)
        {
            var user = await _context.Users.Include(u => u.Faculty).FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                userId = user.UserId,
                matricNo = user.MatricNo,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role,
                facultyId = user.FacultyId,
                facultyCode = user.Faculty?.FacultyCode ?? "",
                facultyName = user.Faculty?.FacultyName ?? ""
            });
        }

        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] MobileProfileUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            if (dto.FacultyId.HasValue) user.FacultyId = dto.FacultyId.Value;

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        // ==========================================
        // 5. MATCH REMINDERS CRUD #2
        // ==========================================
        [HttpGet("reminders/{userId}")]
        public async Task<IActionResult> GetReminders(int userId)
        {
            var list = await _context.MatchReminders
                .Include(r => r.Match)
                    .ThenInclude(m => m!.Sport)
                .Include(r => r.Match)
                    .ThenInclude(m => m!.HomeFaculty)
                .Include(r => r.Match)
                    .ThenInclude(m => m!.AwayFaculty)
                .Include(r => r.Match)
                    .ThenInclude(m => m!.Venue)
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.Match!.MatchDate)
                .ToListAsync();

            var result = list.Select(r => new
            {
                reminderId = r.ReminderId,
                matchId = r.MatchId,
                leadTimeMinutes = r.LeadTimeMinutes,
                createdAt = r.CreatedAt,
                homeFaculty = r.Match?.HomeFaculty?.FacultyCode ?? "HOME",
                awayFaculty = r.Match?.AwayFaculty?.FacultyCode ?? "AWAY",
                sport = r.Match?.Sport?.SportName ?? "Sport",
                venue = r.Match?.Venue?.VenueName ?? "Venue",
                matchDate = r.Match?.MatchDate ?? DateTime.Now,
                status = r.Match?.Status ?? "Scheduled"
            });

            return Ok(result);
        }

        [HttpPost("reminders")]
        public async Task<IActionResult> CreateReminder([FromBody] MobileReminderCreateDto dto)
        {
            var existing = await _context.MatchReminders
                .FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.MatchId == dto.MatchId);

            if (existing != null)
            {
                existing.LeadTimeMinutes = dto.LeadTimeMinutes;
            }
            else
            {
                var reminder = new MatchReminder
                {
                    UserId = dto.UserId,
                    MatchId = dto.MatchId,
                    LeadTimeMinutes = dto.LeadTimeMinutes,
                    CreatedAt = DateTime.Now
                };
                _context.MatchReminders.Add(reminder);
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpDelete("reminders/{reminderId}")]
        public async Task<IActionResult> DeleteReminder(int reminderId)
        {
            var item = await _context.MatchReminders.FindAsync(reminderId);
            if (item != null)
            {
                _context.MatchReminders.Remove(item);
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // ==========================================
        // 6. VENUE FEEDBACK / SAFETY TICKETS CRUD #3
        // ==========================================
        [HttpGet("feedbacks/{userId}")]
        public async Task<IActionResult> GetFeedbacks(int userId)
        {
            var list = await _context.VenueFeedbacks
                .Include(f => f.Venue)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();

            var result = list.Select(f => new
            {
                feedbackId = f.FeedbackId,
                venueId = f.VenueId,
                venueName = f.Venue?.VenueName ?? "Venue",
                subject = f.Subject,
                details = f.Details,
                status = f.Status,
                submittedAt = f.SubmittedAt
            });

            return Ok(result);
        }

        [HttpPost("feedbacks")]
        public async Task<IActionResult> CreateFeedback([FromBody] MobileFeedbackCreateDto dto)
        {
            var feedback = new VenueFeedback
            {
                UserId = dto.UserId,
                VenueId = dto.VenueId,
                Subject = dto.Subject,
                Details = dto.Details,
                Status = "Pending",
                SubmittedAt = DateTime.Now
            };

            _context.VenueFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, feedbackId = feedback.FeedbackId });
        }

        [HttpDelete("feedbacks/{feedbackId}")]
        public async Task<IActionResult> DeleteFeedback(int feedbackId)
        {
            var item = await _context.VenueFeedbacks.FindAsync(feedbackId);
            if (item != null)
            {
                _context.VenueFeedbacks.Remove(item);
                await _context.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        // ==========================================
        // 7. VENUES & PLAYERS (WITH PICTURES)
        // ==========================================
        [HttpGet("venues")]
        public async Task<IActionResult> GetVenues()
        {
            var venues = await _context.Venues.ToListAsync();
            return Ok(venues.Select(v => new
            {
                venueId = v.VenueId,
                venueName = v.VenueName,
                location = v.Location,
                isAvailable = v.IsAvailable,
                imageUrl = v.ImageUrl
            }));
        }

        [HttpGet("players")]
        public async Task<IActionResult> GetPlayers(int? facultyId)
        {
            var query = _context.Players.Include(p => p.Faculty).AsQueryable();
            if (facultyId.HasValue) query = query.Where(p => p.FacultyId == facultyId.Value);

            var players = await query.ToListAsync();
            return Ok(players.Select(p => new
            {
                playerId = p.PlayerId,
                facultyId = p.FacultyId,
                facultyCode = p.Faculty?.FacultyCode,
                fullName = p.FullName,
                matricNo = p.MatricNo,
                positionRole = p.PositionRole,
                photoUrl = p.PhotoUrl
            }));
        }
    }

    public class MobileLoginRequestDto
    {
        public string Identifier { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class MobileProfileUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? FacultyId { get; set; }
    }

    public class MobileReminderCreateDto
    {
        public int UserId { get; set; }
        public int MatchId { get; set; }
        public int LeadTimeMinutes { get; set; }
    }

    public class MobileFeedbackCreateDto
    {
        public int UserId { get; set; }
        public int VenueId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}

