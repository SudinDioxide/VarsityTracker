using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VarsityTracker.Core.Models;
using VarsityTracker.Web.Data;
using VarsityTracker.Web.Models;

namespace VarsityTracker.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly VarsityDbContext _context;

        public HomeController(VarsityDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Live Matches
            var liveMatches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Where(m => m.Status == "Live")
                .ToListAsync();

            // 2. Upcoming Scheduled Matches
            var upcomingMatches = await _context.Matches
                .Include(m => m.Sport)
                .Include(m => m.HomeFaculty)
                .Include(m => m.AwayFaculty)
                .Include(m => m.Venue)
                .Where(m => m.Status == "Scheduled")
                .OrderBy(m => m.MatchDate)
                .Take(5)
                .ToListAsync();

            // 3. System Statistics
            ViewBag.TotalFaculties = await _context.Faculties.CountAsync();
            ViewBag.TotalSports = await _context.Sports.CountAsync();
            ViewBag.TotalPlayers = await _context.Players.CountAsync();
            ViewBag.ActiveVenues = await _context.Venues.CountAsync(v => v.IsAvailable);
            ViewBag.TotalMatches = await _context.Matches.CountAsync();

            // 4. Calculate Dynamic Inter-Faculty Standings
            var faculties = await _context.Faculties.ToListAsync();
            var completedMatches = await _context.Matches
                .Where(m => m.Status == "Completed")
                .ToListAsync();

            var standings = new List<FacultyStandingViewModel>();

            foreach (var f in faculties)
            {
                var row = new FacultyStandingViewModel
                {
                    FacultyId = f.FacultyId,
                    FacultyCode = f.FacultyCode,
                    FacultyName = f.FacultyName,
                    ManagerName = f.ManagerName
                };

                // Home matches
                var homeGames = completedMatches.Where(m => m.HomeFacultyId == f.FacultyId).ToList();
                // Away matches
                var awayGames = completedMatches.Where(m => m.AwayFacultyId == f.FacultyId).ToList();

                row.Played = homeGames.Count + awayGames.Count;

                foreach (var g in homeGames)
                {
                    row.PointsFor += g.HomeTotalScore;
                    row.PointsAgainst += g.AwayTotalScore;

                    if (g.HomeTotalScore > g.AwayTotalScore)
                    {
                        row.Won++;
                        row.Points += 3;
                    }
                    else if (g.HomeTotalScore == g.AwayTotalScore)
                    {
                        row.Drawn++;
                        row.Points += 1;
                    }
                    else
                    {
                        row.Lost++;
                    }
                }

                foreach (var g in awayGames)
                {
                    row.PointsFor += g.AwayTotalScore;
                    row.PointsAgainst += g.HomeTotalScore;

                    if (g.AwayTotalScore > g.HomeTotalScore)
                    {
                        row.Won++;
                        row.Points += 3;
                    }
                    else if (g.AwayTotalScore == g.HomeTotalScore)
                    {
                        row.Drawn++;
                        row.Points += 1;
                    }
                    else
                    {
                        row.Lost++;
                    }
                }

                standings.Add(row);
            }

            // Order by points desc, then point difference desc
            var sortedStandings = standings
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.PointDifference)
                .ThenByDescending(s => s.PointsFor)
                .ToList();

            // Assign ranks
            for (int i = 0; i < sortedStandings.Count; i++)
            {
                sortedStandings[i].Rank = i + 1;
            }

            ViewBag.LiveMatches = liveMatches;
            ViewBag.UpcomingMatches = upcomingMatches;
            ViewBag.Standings = sortedStandings;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Standings - Inter-Faculty Championship Standings & Medal Tally
        public async Task<IActionResult> Standings()
        {
            var faculties = await _context.Faculties.ToListAsync();
            var completedMatches = await _context.Matches
                .Include(m => m.Sport)
                .Where(m => m.Status == "Completed")
                .ToListAsync();

            var standings = new List<FacultyStandingViewModel>();

            foreach (var f in faculties)
            {
                var row = new FacultyStandingViewModel
                {
                    FacultyId = f.FacultyId,
                    FacultyCode = f.FacultyCode,
                    FacultyName = f.FacultyName,
                    ManagerName = f.ManagerName
                };

                var homeGames = completedMatches.Where(m => m.HomeFacultyId == f.FacultyId).ToList();
                var awayGames = completedMatches.Where(m => m.AwayFacultyId == f.FacultyId).ToList();

                row.Played = homeGames.Count + awayGames.Count;

                var allFacultyGames = completedMatches
                    .Where(m => m.HomeFacultyId == f.FacultyId || m.AwayFacultyId == f.FacultyId)
                    .OrderByDescending(m => m.MatchDate)
                    .Take(5)
                    .ToList();

                foreach (var g in allFacultyGames)
                {
                    bool isHome = g.HomeFacultyId == f.FacultyId;
                    int myScore = isHome ? g.HomeTotalScore : g.AwayTotalScore;
                    int oppScore = isHome ? g.AwayTotalScore : g.HomeTotalScore;

                    if (myScore > oppScore) row.RecentForm.Add("W");
                    else if (myScore == oppScore) row.RecentForm.Add("D");
                    else row.RecentForm.Add("L");
                }

                foreach (var g in homeGames)
                {
                    row.PointsFor += g.HomeTotalScore;
                    row.PointsAgainst += g.AwayTotalScore;

                    if (g.HomeTotalScore > g.AwayTotalScore)
                    {
                        row.Won++;
                        row.Points += 3;
                    }
                    else if (g.HomeTotalScore == g.AwayTotalScore)
                    {
                        row.Drawn++;
                        row.Points += 1;
                    }
                    else
                    {
                        row.Lost++;
                    }
                }

                foreach (var g in awayGames)
                {
                    row.PointsFor += g.AwayTotalScore;
                    row.PointsAgainst += g.HomeTotalScore;

                    if (g.AwayTotalScore > g.HomeTotalScore)
                    {
                        row.Won++;
                        row.Points += 3;
                    }
                    else if (g.AwayTotalScore == g.HomeTotalScore)
                    {
                        row.Drawn++;
                        row.Points += 1;
                    }
                    else
                    {
                        row.Lost++;
                    }
                }

                standings.Add(row);
            }

            var sortedStandings = standings
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.PointDifference)
                .ThenByDescending(s => s.PointsFor)
                .ToList();

            for (int i = 0; i < sortedStandings.Count; i++)
            {
                sortedStandings[i].Rank = i + 1;
            }

            ViewBag.TotalCompleted = completedMatches.Count;
            ViewBag.TotalGoals = completedMatches.Sum(m => m.HomeTotalScore + m.AwayTotalScore);
            ViewBag.TotalDisciplines = await _context.Sports.CountAsync();

            return View(sortedStandings);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class FacultyStandingViewModel
    {
        public int Rank { get; set; }
        public int FacultyId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int PointsFor { get; set; }
        public int PointsAgainst { get; set; }
        public int PointDifference => PointsFor - PointsAgainst;
        public int Points { get; set; }
        public List<string> RecentForm { get; set; } = new List<string>();
        public string MedalDisplay => Rank switch
        {
            1 => "🥇 Gold",
            2 => "🥈 Silver",
            3 => "🥉 Bronze",
            _ => $"#{Rank}"
        };
    }
}
