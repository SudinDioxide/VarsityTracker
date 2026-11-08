using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VarsityTracker.Mobile.Models
{
    public class StudentProfile
    {
        public int UserId { get; set; } = 4; // Default to seeded student Farhan
        public string MatricNo { get; set; } = "AI220015";
        public string FullName { get; set; } = "Muhammad Farhan Bin Azlan";
        public string Email { get; set; } = "farhan@siswa.uthm.edu.my";
        public string Role { get; set; } = "Student";
        public int? FacultyId { get; set; } = 1;
        public string FacultyCode { get; set; } = "FSKTM";
        public string FacultyName { get; set; } = "Fakulti Sains Komputer dan Teknologi Maklumat";
    }

    public class MatchItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private int _matchId;
        private string _sport = string.Empty;
        private string _scoringType = "Goals";
        private string _homeFaculty = string.Empty;
        private string _awayFaculty = string.Empty;
        private int _homeScore;
        private int _awayScore;
        private string _venue = string.Empty;
        private DateTime _matchDate;
        private string _status = "Scheduled";
        private string _latestEvent = string.Empty;

        public int MatchId
        {
            get => _matchId;
            set { if (_matchId != value) { _matchId = value; OnPropertyChanged(); } }
        }
        public string Sport
        {
            get => _sport;
            set { if (_sport != value) { _sport = value; OnPropertyChanged(); } }
        }
        public string ScoringType
        {
            get => _scoringType;
            set { if (_scoringType != value) { _scoringType = value; OnPropertyChanged(); } }
        }
        public string HomeFaculty
        {
            get => _homeFaculty;
            set { if (_homeFaculty != value) { _homeFaculty = value; OnPropertyChanged(); OnPropertyChanged(nameof(MatchTitle)); } }
        }
        public string AwayFaculty
        {
            get => _awayFaculty;
            set { if (_awayFaculty != value) { _awayFaculty = value; OnPropertyChanged(); OnPropertyChanged(nameof(MatchTitle)); } }
        }
        public int HomeScore
        {
            get => _homeScore;
            set { if (_homeScore != value) { _homeScore = value; OnPropertyChanged(); OnPropertyChanged(nameof(ScoreDisplay)); } }
        }
        public int AwayScore
        {
            get => _awayScore;
            set { if (_awayScore != value) { _awayScore = value; OnPropertyChanged(); OnPropertyChanged(nameof(ScoreDisplay)); } }
        }
        public string Venue
        {
            get => _venue;
            set { if (_venue != value) { _venue = value; OnPropertyChanged(); } }
        }
        public DateTime MatchDate
        {
            get => _matchDate;
            set { if (_matchDate != value) { _matchDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormattedDate)); } }
        }
        public string Status
        {
            get => _status;
            set 
            { 
                if (_status != value) 
                { 
                    _status = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(ScoreDisplay));
                    OnPropertyChanged(nameof(IsLive));
                    OnPropertyChanged(nameof(StatusColor));
                } 
            }
        }

        public string MatchTitle => $"{HomeFaculty} vs {AwayFaculty}";
        public string ScoreDisplay => Status == "Scheduled" ? "vs" : $"{HomeScore} - {AwayScore}";
        public string FormattedDate => MatchDate.ToString("ddd, dd MMM • h:mm tt");
        public bool IsLive => Status == "Live";
        public string StatusColor => Status == "Live" ? "#DC3545" : Status == "Completed" ? "#198754" : "#0D6EFD";

        public string LatestEvent
        {
            get => _latestEvent;
            set 
            { 
                if (_latestEvent != value) 
                { 
                    _latestEvent = value; 
                    OnPropertyChanged(); 
                    OnPropertyChanged(nameof(HasLatestEvent)); 
                } 
            }
        }
        public bool HasLatestEvent => !string.IsNullOrWhiteSpace(LatestEvent);
    }

    public class ReminderItem
    {
        public int ReminderId { get; set; }
        public int MatchId { get; set; }
        public int LeadTimeMinutes { get; set; } = 30;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string HomeFaculty { get; set; } = string.Empty;
        public string AwayFaculty { get; set; } = string.Empty;
        public string Sport { get; set; } = string.Empty;
        public string Venue { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; }
        public string Status { get; set; } = "Scheduled";

        public string FixtureTitle => $"{HomeFaculty} vs {AwayFaculty}";
        public string FormattedTime => MatchDate.ToString("dd MMM yyyy, h:mm tt");
        public string LeadTimeDisplay => $"Alert: {LeadTimeMinutes} min before kickoff";
    }

    public class FeedbackItem
    {
        public int FeedbackId { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public string FormattedDate => SubmittedAt.ToString("dd MMM yyyy, h:mm tt");
        public string StatusColor => Status == "Resolved" ? "#198754" : Status == "In Review" ? "#FFC107" : "#6C757D";
    }

    public class VenueOption
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    public class FacultyOption
    {
        public int FacultyId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
    }

    public class MobileLoginResultDto
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string MatricNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? FacultyId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
    }

    public class MobileErrorDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class StandingItem
    {
        public int Rank { get; set; }
        public int FacultyId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int PointsFor { get; set; }
        public int PointsAgainst { get; set; }
        public int PointDifference { get; set; }
        public int Points { get; set; }

        public string RankDisplay => Rank switch
        {
            1 => "🥇 1",
            2 => "🥈 2",
            3 => "🥉 3",
            _ => $"#{Rank}"
        };

        public string RankBadgeColor => Rank switch
        {
            1 => "#F59E0B",
            2 => "#64748B",
            3 => "#B45309",
            _ => "#0D2B45"
        };

        public string DiffDisplay => PointDifference > 0 ? $"+{PointDifference}" : $"{PointDifference}";
        public string RecordDisplay => $"{Won}W • {Drawn}D • {Lost}L";
    }

    public class MatchDetailDto
    {
        public int MatchId { get; set; }
        public string Sport { get; set; } = string.Empty;
        public string ScoringType { get; set; } = string.Empty;
        public string HomeFaculty { get; set; } = string.Empty;
        public string HomeFacultyName { get; set; } = string.Empty;
        public string HomeManager { get; set; } = string.Empty;
        public string AwayFaculty { get; set; } = string.Empty;
        public string AwayFacultyName { get; set; } = string.Empty;
        public string AwayManager { get; set; } = string.Empty;
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        public string Venue { get; set; } = string.Empty;
        public string VenueLocation { get; set; } = string.Empty;
        public bool VenueIsAvailable { get; set; } = true;
        public string Scorekeeper { get; set; } = string.Empty;
        public DateTime MatchDate { get; set; }
        public string Status { get; set; } = "Scheduled";
        public System.Collections.Generic.List<PeriodScoreDto> Periods { get; set; } = new System.Collections.Generic.List<PeriodScoreDto>();
        public System.Collections.Generic.List<MatchEventDto> Events { get; set; } = new System.Collections.Generic.List<MatchEventDto>();
        public System.Collections.Generic.List<PlayerDto> HomePlayers { get; set; } = new System.Collections.Generic.List<PlayerDto>();
        public System.Collections.Generic.List<PlayerDto> AwayPlayers { get; set; } = new System.Collections.Generic.List<PlayerDto>();

        public string ScoreDisplay => Status == "Scheduled" ? "vs" : $"{HomeScore} - {AwayScore}";
        public string FormattedDate => MatchDate.ToString("dddd, dd MMMM yyyy • h:mm tt");
        public string StatusColor => Status == "Live" ? "#DC3545" : Status == "Completed" ? "#198754" : "#0D6EFD";
    }

    public class PeriodScoreDto
    {
        public int PeriodNumber { get; set; }
        public int HomePoints { get; set; }
        public int AwayPoints { get; set; }
        public string PeriodLabel => $"Period {PeriodNumber}: {HomePoints} - {AwayPoints}";
    }

    public class MatchEventDto
    {
        public int MatchEventId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Minute { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? PlayerName { get; set; }
        public string Icon => EventType switch
        {
            "Goal" => "⚽",
            "Point" => "🏀",
            "YellowCard" => "🟨",
            "RedCard" => "🟥",
            _ => "📢"
        };
    }

    public class PlayerDto
    {
        public int PlayerId { get; set; }
        public int? FacultyId { get; set; }
        public string? FacultyCode { get; set; }
        public string? FacultyName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string MatricNo { get; set; } = string.Empty;
        public string PositionRole { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }

        public string DisplayPhoto => !string.IsNullOrEmpty(PhotoUrl)
            ? (PhotoUrl.StartsWith("http") ? PhotoUrl : $"http://localhost:5299{PhotoUrl}")
            : "dotnet_bot.png";

        public bool HasPhoto => !string.IsNullOrEmpty(PhotoUrl);

        public string PositionBadge => string.IsNullOrWhiteSpace(PositionRole) ? "Varsity Athlete" : PositionRole;

        public string FacultyDisplay => !string.IsNullOrEmpty(FacultyCode) ? FacultyCode : "UTHM";

        public string Initials => !string.IsNullOrWhiteSpace(FullName)
            ? string.Join("", FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(s => s[0])).ToUpper()
            : "ATH";

        public string FacultyBadgeColor => FacultyCode switch
        {
            "FSKTM" => "#0D2B45",
            "FKEE" => "#0284C7",
            "FKMP" => "#D97706",
            "FAST" => "#059669",
            "FKAAB" => "#7C3AED",
            "FPTP" => "#DC2626",
            _ => "#475569"
        };
    }
}

