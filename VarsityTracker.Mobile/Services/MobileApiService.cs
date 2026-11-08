using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VarsityTracker.Mobile.Models;

namespace VarsityTracker.Mobile.Services
{
    public class MobileApiService
    {
        private static readonly HttpClient _http;

        static MobileApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true,
                AllowAutoRedirect = true
            };
            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2.5) };
            _http.DefaultRequestHeaders.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            _http.DefaultRequestHeaders.Add("Pragma", "no-cache");
        }

        public MobileApiService()
        {
        }

        public static string? _confirmedBaseUrl = null;
        public static bool IsConnectedToBackend { get; set; } = false;
        public static string? ActiveServerUrl { get; set; } = null;
        public static string? LastSyncError { get; set; } = null;
        public static string DiagnosticLog { get; set; } = "Ready";

        public static void SetCustomUrl(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                url = url.Trim().TrimEnd('/');
                if (!url.EndsWith("/api/mobile", StringComparison.OrdinalIgnoreCase))
                {
                    url = $"{url}/api/mobile";
                }
                _confirmedBaseUrl = url;
                IsConnectedToBackend = false;
            }
        }

        // Simple configuration: just the 2 standard ports (HTTPS 7291 primary, HTTP 5299 fallback)
        private static readonly string[] CandidateUrls = DeviceInfo.Platform == DevicePlatform.Android
            ? new[] 
            { 
                "https://10.0.2.2:7291/api/mobile",
                "http://10.0.2.2:5299/api/mobile"
            }
            : new[] 
            { 
                "https://localhost:7291/api/mobile", 
                "http://localhost:5299/api/mobile"
            };

        private string BaseUrl => _confirmedBaseUrl ?? CandidateUrls[0];

        // Centralized fast API GET helper with automatic fallback
        private async Task<T?> GetJsonAsync<T>(string relativePath)
        {
            string path = relativePath.TrimStart('/');

            // 1. Fast path: use already confirmed working URL (<10ms)
            if (!string.IsNullOrEmpty(_confirmedBaseUrl))
            {
                try
                {
                    var data = await _http.GetFromJsonAsync<T>($"{_confirmedBaseUrl}/{path}");
                    if (data != null)
                    {
                        IsConnectedToBackend = true;
                        ActiveServerUrl = _confirmedBaseUrl;
                        LastSyncError = null;
                        return data;
                    }
                }
                catch
                {
                    _confirmedBaseUrl = null;
                    IsConnectedToBackend = false;
                }
            }

            // 2. Probe candidates (only 2 URLs, 2.5s timeout each)
            foreach (var url in CandidateUrls)
            {
                try
                {
                    var data = await _http.GetFromJsonAsync<T>($"{url}/{path}");
                    if (data != null)
                    {
                        _confirmedBaseUrl = url;
                        IsConnectedToBackend = true;
                        ActiveServerUrl = url;
                        LastSyncError = null;
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    LastSyncError = ex.Message;
                }
            }

            IsConnectedToBackend = false;
            return default;
        }

        // In-memory persistent local fallback cache
        private static List<ReminderItem> _localReminders = new List<ReminderItem>
        {
            new ReminderItem
            {
                ReminderId = 1,
                MatchId = 2,
                HomeFaculty = "FKAAB",
                AwayFaculty = "FAST",
                Sport = "Badminton",
                Venue = "Court A - Indoor Badminton Hall",
                MatchDate = DateTime.Now.AddHours(2),
                LeadTimeMinutes = 30,
                Status = "Scheduled"
            },
            new ReminderItem
            {
                ReminderId = 2,
                MatchId = 1,
                HomeFaculty = "FSKTM",
                AwayFaculty = "FKEE",
                Sport = "Futsal",
                Venue = "Futsal Court 1 (Synthetic)",
                MatchDate = DateTime.Now.AddHours(-1),
                LeadTimeMinutes = 15,
                Status = "Live"
            }
        };

        private static List<FeedbackItem> _localFeedbacks = new List<FeedbackItem>
        {
            new FeedbackItem
            {
                FeedbackId = 1,
                VenueId = 5,
                VenueName = "Tennis Court 3",
                Subject = "Damaged Perimeter Netting & Surface Cracks",
                Details = "Perimeter netting near baseline has a 2-meter tear. Slippery surface cracks.",
                Status = "In Review",
                SubmittedAt = DateTime.Now.AddDays(-2)
            },
            new FeedbackItem
            {
                FeedbackId = 2,
                VenueId = 2,
                VenueName = "Court A - Indoor Badminton Hall",
                Subject = "Court 2 Floodlight Flickering",
                Details = "High bay light #4 blinks intermittently during evening practice matches.",
                Status = "Resolved",
                SubmittedAt = DateTime.Now.AddDays(-5)
            }
        };

        private static List<MatchItem> _localMatches = new List<MatchItem>
        {
            new MatchItem
            {
                MatchId = 1,
                Sport = "Futsal",
                ScoringType = "Goals",
                HomeFaculty = "FSKTM",
                AwayFaculty = "FKEE",
                HomeScore = 9,
                AwayScore = 8,
                Venue = "Futsal Court 1 (Synthetic)",
                MatchDate = DateTime.Now.AddHours(-1),
                Status = "Live",
                LatestEvent = "Goal scored!"
            },
            new MatchItem
            {
                MatchId = 3,
                Sport = "Basketball",
                ScoringType = "Points",
                HomeFaculty = "FKMP",
                AwayFaculty = "FSKTM",
                HomeScore = 75,
                AwayScore = 69,
                Venue = "Court B - Basketball Arena",
                MatchDate = DateTime.Now.AddDays(-1),
                Status = "Live",
                LatestEvent = "2-point shot scored"
            },
            new MatchItem
            {
                MatchId = 4,
                Sport = "Volleyball",
                ScoringType = "Sets",
                HomeFaculty = "FSKTM",
                AwayFaculty = "FKMP",
                HomeScore = 2,
                AwayScore = 0,
                Venue = "Main Stadium UTHM",
                MatchDate = DateTime.Now.AddDays(1),
                Status = "Live"
            }
        };

        // ==========================================
        // MATCHES & LIVE SCORE VIEWER
        // ==========================================

        public async Task<List<MatchItem>> GetMatchesAsync()
        {
            string cacheBuster = $"?t={DateTime.UtcNow.Ticks}";
            var matches = await GetJsonAsync<List<MatchItem>>($"matches{cacheBuster}");
            if (matches != null && matches.Any())
            {
                _localMatches = matches;
                return matches;
            }
            return _localMatches.OrderByDescending(m => m.IsLive).ThenBy(m => m.MatchDate).ToList();
        }

        public async Task<MatchDetailDto?> GetMatchDetailAsync(int matchId)
        {
            string cacheBuster = $"?t={DateTime.UtcNow.Ticks}";
            var detail = await GetJsonAsync<MatchDetailDto>($"matches/{matchId}{cacheBuster}");
            if (detail != null) return detail;

            // Local fallback
            var m = _localMatches.FirstOrDefault(x => x.MatchId == matchId);
            if (m == null) return null;

            return new MatchDetailDto
            {
                MatchId = m.MatchId,
                Sport = m.Sport,
                ScoringType = m.ScoringType,
                HomeFaculty = m.HomeFaculty,
                HomeFacultyName = GetPredefinedFaculties().FirstOrDefault(f => f.FacultyCode == m.HomeFaculty)?.FacultyName ?? m.HomeFaculty,
                AwayFaculty = m.AwayFaculty,
                AwayFacultyName = GetPredefinedFaculties().FirstOrDefault(f => f.FacultyCode == m.AwayFaculty)?.FacultyName ?? m.AwayFaculty,
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                Venue = m.Venue,
                VenueLocation = "Kompleks Sukan UTHM",
                VenueIsAvailable = true,
                Scorekeeper = "Official Scorekeeper (UTHM)",
                MatchDate = m.MatchDate,
                Status = m.Status,
                Periods = new List<PeriodScoreDto>
                {
                    new PeriodScoreDto { PeriodNumber = 1, HomePoints = m.HomeScore, AwayPoints = m.AwayScore }
                },
                Events = new List<MatchEventDto>
                {
                    new MatchEventDto { MatchEventId = 1, EventType = "Goal", Minute = "14'", Description = "Match active and underway" }
                }
            };
        }

        // ==========================================
        // INTER-FACULTY CHAMPIONSHIP STANDINGS
        // ==========================================

        private static List<StandingItem> _localStandings = new List<StandingItem>
        {
            new StandingItem { Rank = 1, FacultyId = 1, FacultyCode = "FSKTM", FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat", Played = 2, Won = 2, Drawn = 0, Lost = 0, PointsFor = 70, PointsAgainst = 63, PointDifference = 7, Points = 6 },
            new StandingItem { Rank = 2, FacultyId = 3, FacultyCode = "FKMP", FacultyName = "Fakulti Kejuruteraan Mekanikal dan Pembuatan", Played = 2, Won = 1, Drawn = 0, Lost = 1, PointsFor = 68, PointsAgainst = 62, PointDifference = 6, Points = 3 },
            new StandingItem { Rank = 3, FacultyId = 2, FacultyCode = "FKEE", FacultyName = "Fakulti Kejuruteraan Elektrik dan Elektronik", Played = 2, Won = 1, Drawn = 0, Lost = 1, PointsFor = 3, PointsAgainst = 2, PointDifference = 1, Points = 3 },
            new StandingItem { Rank = 4, FacultyId = 4, FacultyCode = "FKAAB", FacultyName = "Fakulti Kejuruteraan Awam dan Alam Bina", Played = 1, Won = 0, Drawn = 0, Lost = 1, PointsFor = 0, PointsAgainst = 0, PointDifference = 0, Points = 0 },
            new StandingItem { Rank = 5, FacultyId = 5, FacultyCode = "FAST", FacultyName = "Fakulti Sains Gunaan dan Teknologi", Played = 1, Won = 0, Drawn = 0, Lost = 1, PointsFor = 0, PointsAgainst = 0, PointDifference = 0, Points = 0 },
            new StandingItem { Rank = 6, FacultyId = 6, FacultyCode = "FPTP", FacultyName = "Fakulti Pengurusan Teknologi dan Perniagaan", Played = 0, Won = 0, Drawn = 0, Lost = 0, PointsFor = 0, PointsAgainst = 0, PointDifference = 0, Points = 0 }
        };

        public async Task<List<StandingItem>> GetStandingsAsync()
        {
            var list = await GetJsonAsync<List<StandingItem>>("standings");
            if (list != null && list.Any())
            {
                _localStandings = list;
                return list;
            }
            return _localStandings.OrderBy(s => s.Rank).ToList();
        }

        // ==========================================
        // ATHLETES & PLAYERS ROSTER
        // ==========================================

        private static List<PlayerDto> _localPlayers = new List<PlayerDto>
        {
            new PlayerDto { PlayerId = 1, FacultyId = 1, FacultyCode = "FSKTM", FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat", FullName = "Danish Hakim", MatricNo = "AI220044", PositionRole = "Pivot / Striker", PhotoUrl = "/uploads/players/danish.jpg" },
            new PlayerDto { PlayerId = 2, FacultyId = 1, FacultyCode = "FSKTM", FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat", FullName = "Luqman Haziq", MatricNo = "AI220112", PositionRole = "Goalkeeper", PhotoUrl = "/uploads/players/luqman.jpg" },
            new PlayerDto { PlayerId = 3, FacultyId = 1, FacultyCode = "FSKTM", FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat", FullName = "Adam Firdaus", MatricNo = "AI220188", PositionRole = "Defender", PhotoUrl = "/uploads/players/adam.jpg" },
            new PlayerDto { PlayerId = 4, FacultyId = 2, FacultyCode = "FKEE", FacultyName = "Fakulti Kejuruteraan Elektrik dan Elektronik", FullName = "Harith Zikri", MatricNo = "CI220033", PositionRole = "Striker", PhotoUrl = "/uploads/players/harith.jpg" },
            new PlayerDto { PlayerId = 5, FacultyId = 2, FacultyCode = "FKEE", FacultyName = "Fakulti Kejuruteraan Elektrik dan Elektronik", FullName = "Hafiz Ridzuan", MatricNo = "CI220055", PositionRole = "Defender", PhotoUrl = "/uploads/players/hafiz.jpg" },
            new PlayerDto { PlayerId = 6, FacultyId = 3, FacultyCode = "FKMP", FacultyName = "Fakulti Kejuruteraan Mekanikal dan Pembuatan", FullName = "Syahmi Naufal", MatricNo = "CD220077", PositionRole = "Point Guard", PhotoUrl = "/uploads/players/syahmi.jpg" },
            new PlayerDto { PlayerId = 7, FacultyId = 5, FacultyCode = "FAST", FacultyName = "Fakulti Sains Gunaan dan Teknologi", FullName = "Daniel Wong", MatricNo = "DI220021", PositionRole = "Forward", PhotoUrl = "/uploads/players/daniel.jpg" },
            new PlayerDto { PlayerId = 8, FacultyId = 4, FacultyCode = "FKAAB", FacultyName = "Fakulti Kejuruteraan Awam dan Alam Bina", FullName = "Ahmad Khairi", MatricNo = "AA220090", PositionRole = "Setter / Spiker", PhotoUrl = null },
            new PlayerDto { PlayerId = 9, FacultyId = 6, FacultyCode = "FPTP", FacultyName = "Fakulti Pengurusan Teknologi dan Perniagaan", FullName = "Zulhilmi Faiz", MatricNo = "BP220018", PositionRole = "Shooting Guard", PhotoUrl = null }
        };

        public async Task<List<PlayerDto>> GetPlayersAsync(int? facultyId = null)
        {
            string endpoint = facultyId.HasValue 
                ? $"players?facultyId={facultyId.Value}&t={DateTime.UtcNow.Ticks}" 
                : $"players?t={DateTime.UtcNow.Ticks}";

            var list = await GetJsonAsync<List<PlayerDto>>(endpoint);
            if (list != null && list.Any())
            {
                return list;
            }

            var query = _localPlayers.AsEnumerable();
            if (facultyId.HasValue)
            {
                query = query.Where(p => p.FacultyId == facultyId.Value);
            }
            return query.ToList();
        }

        // ==========================================
        // MOBILE CRUD #1: STUDENT PROFILE & AUTH
        // ==========================================

        public async Task<(bool success, string message, StudentProfile? profile)> LoginAsync(string identifier, string password)
        {
            var urlsToTry = !string.IsNullOrEmpty(_confirmedBaseUrl)
                ? new[] { _confirmedBaseUrl }
                : CandidateUrls;

            foreach (var url in urlsToTry)
            {
                try
                {
                    var response = await _http.PostAsJsonAsync($"{url}/login", new
                    {
                        identifier = identifier.Trim(),
                        password = password.Trim()
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        _confirmedBaseUrl = url;
                        var result = await response.Content.ReadFromJsonAsync<MobileLoginResultDto>();
                        if (result != null && result.Success)
                        {
                            var profile = new StudentProfile
                            {
                                UserId = result.UserId,
                                MatricNo = result.MatricNo,
                                FullName = result.FullName,
                                Email = result.Email,
                                Role = result.Role,
                                FacultyId = result.FacultyId,
                                FacultyCode = result.FacultyCode,
                                FacultyName = result.FacultyName
                            };
                            StudentSession.Current.Profile = profile;
                            return (true, "Login successful.", profile);
                        }
                    }
                    else
                    {
                        var err = await response.Content.ReadFromJsonAsync<MobileErrorDto>();
                        return (false, err?.Message ?? "Invalid matric number or password.", null);
                    }
                }
                catch
                {
                    if (!string.IsNullOrEmpty(_confirmedBaseUrl)) break;
                }
            }

            // Offline check: reject Admin or Scorekeeper if they try to log into mobile
            if (identifier.Equals("AD001", StringComparison.OrdinalIgnoreCase) || identifier.StartsWith("admin", StringComparison.OrdinalIgnoreCase) ||
                identifier.Equals("SK001", StringComparison.OrdinalIgnoreCase) || identifier.StartsWith("referee", StringComparison.OrdinalIgnoreCase) ||
                identifier.Equals("SK002", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Access Restricted: Administrator and Scorekeeper accounts must log in via the Web Management Portal. The Mobile App is reserved exclusively for Students.", null);
            }

            // Local fallback if offline (Students only)
            if (identifier.Equals("AI220015", StringComparison.OrdinalIgnoreCase) || identifier.StartsWith("farhan", StringComparison.OrdinalIgnoreCase))
            {
                var profile = new StudentProfile
                {
                    UserId = 4,
                    MatricNo = "AI220015",
                    FullName = "Muhammad Farhan Bin Azlan",
                    Email = "farhan@siswa.uthm.edu.my",
                    Role = "Student",
                    FacultyId = 1,
                    FacultyCode = "FSKTM",
                    FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat"
                };
                StudentSession.Current.Profile = profile;
                return (true, "Logged in as Farhan (FSKTM).", profile);
            }
            else if (identifier.Equals("CI220098", StringComparison.OrdinalIgnoreCase) || identifier.StartsWith("aisyah", StringComparison.OrdinalIgnoreCase))
            {
                var profile = new StudentProfile
                {
                    UserId = 5,
                    MatricNo = "CI220098",
                    FullName = "Nur Aisyah Binti Zulkifli",
                    Email = "aisyah@siswa.uthm.edu.my",
                    Role = "Student",
                    FacultyId = 2,
                    FacultyCode = "FKEE",
                    FacultyName = "Fakulti Kejuruteraan Elektrik dan Elektronik"
                };
                StudentSession.Current.Profile = profile;
                return (true, "Logged in as Aisyah (FKEE).", profile);
            }

            return (false, "Unable to connect to authentication server. Please check your credentials or network.", null);
        }

        public async Task<StudentProfile> GetProfileAsync(int userId)
        {
            try
            {
                var p = await _http.GetFromJsonAsync<StudentProfile>($"{BaseUrl}/profile/{userId}");
                if (p != null) return p;
            }
            catch { }

            return StudentSession.Current.Profile;
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string email, int? facultyId, string? facultyCode)
        {
            try
            {
                var res = await _http.PutAsJsonAsync($"{BaseUrl}/profile/{userId}", new
                {
                    fullName = fullName,
                    email = email,
                    facultyId = facultyId
                });
            }
            catch { }

            StudentSession.Current.UpdateProfile(fullName, email, facultyId, facultyCode);
            return true;
        }

        public async Task<bool> DeleteProfileAsync(int userId)
        {
            try
            {
                await _http.DeleteAsync($"{BaseUrl}/profile/{userId}");
            }
            catch { }

            StudentSession.Current.Logout();
            _localReminders.Clear();
            _localFeedbacks.Clear();
            return true;
        }

        public async Task<bool> RegisterProfileAsync(string matricNo, string fullName, string email, int facultyId, string password)
        {
            try
            {
                var res = await _http.PostAsJsonAsync($"{BaseUrl}/profile/register", new
                {
                    matricNo, fullName, email, facultyId, password
                });
            }
            catch { }

            StudentSession.Current.Profile = new StudentProfile
            {
                UserId = 10,
                MatricNo = matricNo,
                FullName = fullName,
                Email = email,
                FacultyId = facultyId,
                FacultyCode = GetFacultyCodeById(facultyId)
            };
            return true;
        }

        // ==========================================
        // MOBILE CRUD #2: PERSONAL MATCH REMINDERS
        // ==========================================

        public async Task<List<ReminderItem>> GetRemindersAsync(int userId)
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<ReminderItem>>($"{BaseUrl}/reminders/{userId}");
                if (list != null)
                {
                    _localReminders = list;
                    return list;
                }
            }
            catch { }

            return _localReminders;
        }

        public async Task<bool> CreateReminderAsync(int userId, int matchId, int leadTimeMinutes)
        {
            try
            {
                await _http.PostAsJsonAsync($"{BaseUrl}/reminders", new
                {
                    userId, matchId, leadTimeMinutes
                });
            }
            catch { }

            var match = _localMatches.FirstOrDefault(m => m.MatchId == matchId);
            var existing = _localReminders.FirstOrDefault(r => r.MatchId == matchId);
            if (existing != null)
            {
                existing.LeadTimeMinutes = leadTimeMinutes;
            }
            else
            {
                _localReminders.Add(new ReminderItem
                {
                    ReminderId = _localReminders.Count + 1,
                    MatchId = matchId,
                    LeadTimeMinutes = leadTimeMinutes,
                    CreatedAt = DateTime.Now,
                    HomeFaculty = match?.HomeFaculty ?? "HOME",
                    AwayFaculty = match?.AwayFaculty ?? "AWAY",
                    Sport = match?.Sport ?? "Sport",
                    Venue = match?.Venue ?? "Venue",
                    MatchDate = match?.MatchDate ?? DateTime.Now.AddHours(2),
                    Status = match?.Status ?? "Scheduled"
                });
            }

            return true;
        }

        public async Task<bool> UpdateReminderAsync(int reminderId, int newLeadTimeMinutes)
        {
            try
            {
                await _http.PutAsJsonAsync($"{BaseUrl}/reminders/{reminderId}", new
                {
                    leadTimeMinutes = newLeadTimeMinutes
                });
            }
            catch { }

            var r = _localReminders.FirstOrDefault(x => x.ReminderId == reminderId);
            if (r != null)
            {
                r.LeadTimeMinutes = newLeadTimeMinutes;
            }

            return true;
        }

        public async Task<bool> DeleteReminderAsync(int reminderId)
        {
            try
            {
                await _http.DeleteAsync($"{BaseUrl}/reminders/{reminderId}");
            }
            catch { }

            var item = _localReminders.FirstOrDefault(r => r.ReminderId == reminderId);
            if (item != null)
            {
                _localReminders.Remove(item);
            }

            return true;
        }

        // ==========================================
        // MOBILE CRUD #3: VENUE FEEDBACK TICKETS
        // ==========================================

        public async Task<List<FeedbackItem>> GetFeedbacksAsync(int userId)
        {
            try
            {
                var list = await _http.GetFromJsonAsync<List<FeedbackItem>>($"{BaseUrl}/feedbacks/{userId}");
                if (list != null)
                {
                    _localFeedbacks = list;
                    return list;
                }
            }
            catch { }

            return _localFeedbacks;
        }

        public async Task<bool> CreateFeedbackAsync(int userId, int venueId, string subject, string details)
        {
            try
            {
                await _http.PostAsJsonAsync($"{BaseUrl}/feedbacks", new
                {
                    userId, venueId, subject, details
                });
            }
            catch { }

            var venues = GetPredefinedVenues();
            var venue = venues.FirstOrDefault(v => v.VenueId == venueId);

            _localFeedbacks.Insert(0, new FeedbackItem
            {
                FeedbackId = _localFeedbacks.Count + 1,
                VenueId = venueId,
                VenueName = venue?.VenueName ?? "Campus Venue",
                Subject = subject,
                Details = details,
                Status = "Pending",
                SubmittedAt = DateTime.Now
            });

            return true;
        }

        public async Task<bool> UpdateFeedbackAsync(int feedbackId, int venueId, string subject, string details)
        {
            try
            {
                await _http.PutAsJsonAsync($"{BaseUrl}/feedbacks/{feedbackId}", new
                {
                    venueId, subject, details
                });
            }
            catch { }

            var item = _localFeedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
            if (item != null)
            {
                var venues = GetPredefinedVenues();
                var v = venues.FirstOrDefault(x => x.VenueId == venueId);
                item.VenueId = venueId;
                if (v != null) item.VenueName = v.VenueName;
                item.Subject = subject;
                item.Details = details;
            }

            return true;
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            try
            {
                await _http.DeleteAsync($"{BaseUrl}/feedbacks/{feedbackId}");
            }
            catch { }

            var item = _localFeedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId);
            if (item != null)
            {
                _localFeedbacks.Remove(item);
            }

            return true;
        }

        public List<VenueOption> GetPredefinedVenues()
        {
            return new List<VenueOption>
            {
                new VenueOption { VenueId = 1, VenueName = "Main Stadium UTHM", Location = "Kompleks Sukan UTHM", IsAvailable = true },
                new VenueOption { VenueId = 2, VenueName = "Court A - Indoor Badminton Hall", Location = "Dewan Kolej Perwira", IsAvailable = true },
                new VenueOption { VenueId = 3, VenueName = "Court B - Basketball Arena", Location = "Sports Complex Zone 2", IsAvailable = true },
                new VenueOption { VenueId = 4, VenueName = "Futsal Court 1 (Synthetic)", Location = "Sports Arena Parit Raja", IsAvailable = true },
                new VenueOption { VenueId = 5, VenueName = "Tennis Court 3", Location = "Kompleks Gelanggang Tenis", IsAvailable = false }
            };
        }

        public List<FacultyOption> GetPredefinedFaculties()
        {
            return new List<FacultyOption>
            {
                new FacultyOption { FacultyId = 1, FacultyCode = "FSKTM", FacultyName = "Fakulti Sains Komputer dan Teknologi Maklumat" },
                new FacultyOption { FacultyId = 2, FacultyCode = "FKEE", FacultyName = "Fakulti Kejuruteraan Elektrik dan Elektronik" },
                new FacultyOption { FacultyId = 3, FacultyCode = "FKMP", FacultyName = "Fakulti Kejuruteraan Mekanikal dan Pembuatan" },
                new FacultyOption { FacultyId = 4, FacultyCode = "FKAAB", FacultyName = "Fakulti Kejuruteraan Awam dan Alam Bina" },
                new FacultyOption { FacultyId = 5, FacultyCode = "FAST", FacultyName = "Fakulti Sains Gunaan dan Teknologi" },
                new FacultyOption { FacultyId = 6, FacultyCode = "FPTP", FacultyName = "Fakulti Pengurusan Teknologi dan Perniagaan" }
            };
        }

        private string GetFacultyCodeById(int id)
        {
            return GetPredefinedFaculties().FirstOrDefault(f => f.FacultyId == id)?.FacultyCode ?? "FSKTM";
        }
    }
}

