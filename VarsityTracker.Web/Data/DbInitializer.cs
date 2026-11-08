using System;
using System.Collections.Generic;
using System.Linq;
using VarsityTracker.Core.Models;

namespace VarsityTracker.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(VarsityDbContext context)
        {
            context.Database.EnsureCreated();

            // Safely execute schema migration for new columns and tables in SQLite
            try { Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database, "ALTER TABLE Players ADD COLUMN PhotoUrl TEXT;"); } catch { }
            try { Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database, "ALTER TABLE Venues ADD COLUMN ImageUrl TEXT;"); } catch { }
            try
            {
                Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(context.Database,
                    @"CREATE TABLE IF NOT EXISTS MatchEvents (
                        MatchEventId INTEGER PRIMARY KEY AUTOINCREMENT,
                        MatchId INTEGER NOT NULL,
                        FacultyId INTEGER NULL,
                        EventType TEXT NOT NULL,
                        Minute TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        PlayerName TEXT NULL,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (MatchId) REFERENCES Matches (MatchId) ON DELETE CASCADE,
                        FOREIGN KEY (FacultyId) REFERENCES Faculties (FacultyId) ON DELETE SET NULL
                    );");
            }
            catch { }

            // Look for any existing faculties
            if (context.Faculties.Any())
            {
                // Backfill sample photos for existing venues/players if null
                try
                {
                    var existingVenues = context.Venues.Where(v => string.IsNullOrEmpty(v.ImageUrl)).ToList();
                    var venueImages = new Dictionary<string, string>
                    {
                        { "Main Stadium UTHM", "https://images.unsplash.com/photo-1577223625816-7546f13df25d?w=600&auto=format&fit=crop&q=80" },
                        { "Court A - Indoor Badminton Hall", "https://images.unsplash.com/photo-1626224583764-f87db24ac4ea?w=600&auto=format&fit=crop&q=80" },
                        { "Court B - Basketball Arena", "https://images.unsplash.com/photo-1546519638-68e109498ffc?w=600&auto=format&fit=crop&q=80" },
                        { "Futsal Court 1 (Synthetic)", "https://images.unsplash.com/photo-1529900245534-47fbf8204bca?w=600&auto=format&fit=crop&q=80" },
                        { "Tennis Court 3", "https://images.unsplash.com/photo-1554068865-24cecd4e34b8?w=600&auto=format&fit=crop&q=80" }
                    };
                    foreach (var v in existingVenues)
                    {
                        if (venueImages.TryGetValue(v.VenueName, out var img))
                        {
                            v.ImageUrl = img;
                        }
                    }

                    var existingPlayers = context.Players.Where(p => string.IsNullOrEmpty(p.PhotoUrl)).ToList();
                    var sampleAvatars = new[]
                    {
                        "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=300&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=300&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=300&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?w=300&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=300&auto=format&fit=crop&q=80",
                        "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce?w=300&auto=format&fit=crop&q=80"
                    };
                    int idx = 0;
                    foreach (var p in existingPlayers)
                    {
                        p.PhotoUrl = sampleAvatars[idx % sampleAvatars.Length];
                        idx++;
                    }

                    // Seed sample events if none exist
                    if (!context.MatchEvents.Any())
                    {
                        var firstMatch = context.Matches.FirstOrDefault();
                        if (firstMatch != null)
                        {
                            context.MatchEvents.AddRange(new[]
                            {
                                new MatchEvent { MatchId = firstMatch.MatchId, FacultyId = firstMatch.HomeFacultyId, EventType = "PeriodStart", Minute = "0'", Description = "Match Kick-off (Period 1)", PlayerName = "Referee" },
                                new MatchEvent { MatchId = firstMatch.MatchId, FacultyId = firstMatch.HomeFacultyId, EventType = "Goal", Minute = "12'", Description = "Goal! Low driven strike into bottom corner", PlayerName = "Muhammad Afiq" },
                                new MatchEvent { MatchId = firstMatch.MatchId, FacultyId = firstMatch.AwayFacultyId, EventType = "YellowCard", Minute = "27'", Description = "Caution for tactical foul near midfield", PlayerName = "Farhan Danial" },
                                new MatchEvent { MatchId = firstMatch.MatchId, FacultyId = firstMatch.AwayFacultyId, EventType = "Goal", Minute = "44'", Description = "Goal! Header from set piece equalizer", PlayerName = "Ahmad Haziq" },
                                new MatchEvent { MatchId = firstMatch.MatchId, FacultyId = firstMatch.HomeFacultyId, EventType = "PeriodEnd", Minute = "45'", Description = "Halftime Concluded", PlayerName = "Referee" }
                            });
                        }
                    }

                    context.SaveChanges();
                }
                catch { }

                return; // DB has been seeded already
            }

            // 1. Seed Faculties
            var faculties = new Faculty[]
            {
                new Faculty { FacultyCode = "FSKTM", FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat", ManagerName = "Dr. Radzi Bin Ahmad" },
                new Faculty { FacultyCode = "FKEE", FacultyName = "Fakulti Kejuruteraan Elektrik dan Elektronik", ManagerName = "Ir. Ts. Kamaruzaman" },
                new Faculty { FacultyCode = "FKMP", FacultyName = "Fakulti Kejuruteraan Mekanikal dan Pembuatan", ManagerName = "En. Hafeez Bin Roslan" },
                new Faculty { FacultyCode = "FKAAB", FacultyName = "Fakulti Kejuruteraan Awam dan Alam Bina", ManagerName = "Dr. Norhafizah Binti Salleh" },
                new Faculty { FacultyCode = "FAST", FacultyName = "Fakulti Sains Gunaan dan Teknologi", ManagerName = "Pn. Azlina Binti Mansor" },
                new Faculty { FacultyCode = "FPTP", FacultyName = "Fakulti Pengurusan Teknologi dan Perniagaan", ManagerName = "En. Syahmi Bin Ismail" }
            };
            context.Faculties.AddRange(faculties);
            context.SaveChanges();

            // 2. Seed Sports
            var sports = new Sport[]
            {
                new Sport { SportName = "Futsal", ScoringType = "Goals", MaxPeriods = 2 },
                new Sport { SportName = "Badminton", ScoringType = "Sets", MaxPeriods = 3 },
                new Sport { SportName = "Basketball", ScoringType = "Points", MaxPeriods = 4 },
                new Sport { SportName = "Volleyball", ScoringType = "Sets", MaxPeriods = 5 },
                new Sport { SportName = "Sepak Takraw", ScoringType = "Sets", MaxPeriods = 3 }
            };
            context.Sports.AddRange(sports);
            context.SaveChanges();

            // 3. Seed Venues
            var venues = new Venue[]
            {
                new Venue { VenueName = "Main Stadium UTHM", Location = "Kompleks Sukan UTHM Parit Raja", IsAvailable = true },
                new Venue { VenueName = "Court A - Indoor Badminton Hall", Location = "Dewan Kolej Kediaman Perwira", IsAvailable = true },
                new Venue { VenueName = "Court B - Basketball Arena", Location = "Sports Complex Zone 2", IsAvailable = true },
                new Venue { VenueName = "Futsal Court 1 (Synthetic)", Location = "Kompleks Sukan Terbuka", IsAvailable = true },
                new Venue { VenueName = "Tennis Court 3", Location = "Kompleks Gelanggang Tenis", IsAvailable = false } // Maintenance example
            };
            context.Venues.AddRange(venues);
            context.SaveChanges();

            // 4. Seed Users (Admin, Scorekeepers, Students)
            var users = new User[]
            {
                new User
                {
                    MatricNo = "AD001",
                    FullName = "Varsity Sports Administrator",
                    Email = "admin@uthm.edu.my",
                    PasswordHash = "admin123",
                    Role = "Admin",
                    FacultyId = null
                },
                new User
                {
                    MatricNo = "SK001",
                    FullName = "Amirul Hakim (Official Referee)",
                    Email = "referee1@uthm.edu.my",
                    PasswordHash = "score123",
                    Role = "Scorekeeper",
                    FacultyId = null
                },
                new User
                {
                    MatricNo = "SK002",
                    FullName = "Siti Nurhaliza (Chief Table Official)",
                    Email = "referee2@uthm.edu.my",
                    PasswordHash = "score123",
                    Role = "Scorekeeper",
                    FacultyId = null
                },
                new User
                {
                    MatricNo = "AI220015",
                    FullName = "Muhammad Farhan Bin Azlan",
                    Email = "farhan@siswa.uthm.edu.my",
                    PasswordHash = "student123",
                    Role = "Student",
                    FacultyId = faculties[0].FacultyId // FSKTM
                },
                new User
                {
                    MatricNo = "CI220098",
                    FullName = "Nur Aisyah Binti Zulkifli",
                    Email = "aisyah@siswa.uthm.edu.my",
                    PasswordHash = "student123",
                    Role = "Student",
                    FacultyId = faculties[1].FacultyId // FKEE
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // 5. Seed Players
            var players = new Player[]
            {
                new Player { FacultyId = faculties[0].FacultyId, FullName = "Danish Hakim", MatricNo = "AI220044", PositionRole = "Pivot / Striker" },
                new Player { FacultyId = faculties[0].FacultyId, FullName = "Luqman Haziq", MatricNo = "AI220112", PositionRole = "Goalkeeper" },
                new Player { FacultyId = faculties[0].FacultyId, FullName = "Adam Firdaus", MatricNo = "AI220188", PositionRole = "Defender" },
                new Player { FacultyId = faculties[1].FacultyId, FullName = "Harith Zikri", MatricNo = "CI220033", PositionRole = "Striker" },
                new Player { FacultyId = faculties[1].FacultyId, FullName = "Hafiz Ridzuan", MatricNo = "CI220055", PositionRole = "Defender" },
                new Player { FacultyId = faculties[2].FacultyId, FullName = "Syahmi Naufal", MatricNo = "CD220077", PositionRole = "Point Guard" },
                new Player { FacultyId = faculties[4].FacultyId, FullName = "Daniel Wong", MatricNo = "DI220021", PositionRole = "Forward" }
            };
            context.Players.AddRange(players);
            context.SaveChanges();

            // 6. Seed Matches
            var match1 = new Match
            {
                SportId = sports[0].SportId, // Futsal
                HomeFacultyId = faculties[0].FacultyId, // FSKTM
                AwayFacultyId = faculties[1].FacultyId, // FKEE
                VenueId = venues[3].VenueId, // Futsal Court 1
                ScorekeeperId = users[1].UserId, // Amirul Hakim
                MatchDate = DateTime.Now.AddHours(-1),
                Status = "Live",
                HomeTotalScore = 2,
                AwayTotalScore = 1
            };

            var match2 = new Match
            {
                SportId = sports[1].SportId, // Badminton
                HomeFacultyId = faculties[3].FacultyId, // FKAAB
                AwayFacultyId = faculties[4].FacultyId, // FAST
                VenueId = venues[1].VenueId, // Court A
                ScorekeeperId = users[2].UserId, // Siti Nurhaliza
                MatchDate = DateTime.Now.AddHours(2),
                Status = "Scheduled",
                HomeTotalScore = 0,
                AwayTotalScore = 0
            };

            var match3 = new Match
            {
                SportId = sports[2].SportId, // Basketball
                HomeFacultyId = faculties[2].FacultyId, // FKMP
                AwayFacultyId = faculties[0].FacultyId, // FSKTM
                VenueId = venues[2].VenueId, // Court B
                ScorekeeperId = users[1].UserId,
                MatchDate = DateTime.Now.AddDays(-1),
                Status = "Completed",
                HomeTotalScore = 68,
                AwayTotalScore = 62
            };

            context.Matches.AddRange(match1, match2, match3);
            context.SaveChanges();

            // 7. Seed MatchScores for Match 1 & Match 3
            var matchScores = new MatchScore[]
            {
                new MatchScore { MatchId = match1.MatchId, PeriodNumber = 1, HomePoints = 1, AwayPoints = 0 },
                new MatchScore { MatchId = match1.MatchId, PeriodNumber = 2, HomePoints = 1, AwayPoints = 1 },
                new MatchScore { MatchId = match3.MatchId, PeriodNumber = 1, HomePoints = 18, AwayPoints = 14 },
                new MatchScore { MatchId = match3.MatchId, PeriodNumber = 2, HomePoints = 16, AwayPoints = 15 },
                new MatchScore { MatchId = match3.MatchId, PeriodNumber = 3, HomePoints = 14, AwayPoints = 18 },
                new MatchScore { MatchId = match3.MatchId, PeriodNumber = 4, HomePoints = 20, AwayPoints = 15 }
            };
            context.MatchScores.AddRange(matchScores);

            // 8. Seed MatchReminders
            var reminders = new MatchReminder[]
            {
                new MatchReminder { UserId = users[3].UserId, MatchId = match2.MatchId, LeadTimeMinutes = 30, CreatedAt = DateTime.Now.AddDays(-1) },
                new MatchReminder { UserId = users[4].UserId, MatchId = match1.MatchId, LeadTimeMinutes = 15, CreatedAt = DateTime.Now.AddDays(-1) }
            };
            context.MatchReminders.AddRange(reminders);

            // 9. Seed VenueFeedbacks
            var feedbacks = new VenueFeedback[]
            {
                new VenueFeedback
                {
                    UserId = users[3].UserId,
                    VenueId = venues[4].VenueId, // Tennis Court 3
                    Subject = "Damaged Perimeter Netting & Surface Cracks",
                    Details = "Perimeter netting near baseline has a 2-meter tear. Playing surface shows several slippery cracks after rain.",
                    Status = "In Review",
                    SubmittedAt = DateTime.Now.AddDays(-2)
                },
                new VenueFeedback
                {
                    UserId = users[4].UserId,
                    VenueId = venues[1].VenueId, // Court A
                    Subject = "Court 2 Floodlight Flickering",
                    Details = "High bay light #4 blinks intermittently during evening practice matches.",
                    Status = "Resolved",
                    SubmittedAt = DateTime.Now.AddDays(-5)
                }
            };
            context.VenueFeedbacks.AddRange(feedbacks);

            context.SaveChanges();
        }
    }
}

