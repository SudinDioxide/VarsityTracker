using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class Match
    {
        [Key]
        public int MatchId { get; set; }

        [Required]
        [Display(Name = "Sport")]
        public int SportId { get; set; } // Foreign Key -> Sport

        [Required]
        [Display(Name = "Home Faculty")]
        public int HomeFacultyId { get; set; } // Foreign Key -> Faculty (Home)

        [Required]
        [Display(Name = "Away Faculty")]
        public int AwayFacultyId { get; set; } // Foreign Key -> Faculty (Away)

        [Required]
        [Display(Name = "Venue")]
        public int VenueId { get; set; } // Foreign Key -> Venue

        // Assigned Match Official (Scorekeeper Role)
        [Display(Name = "Scorekeeper Official")]
        public int? ScorekeeperId { get; set; } // Foreign Key -> User (Role = "Scorekeeper")

        [Required]
        [Display(Name = "Match Date & Time")]
        public DateTime MatchDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(30)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Scheduled"; // "Scheduled", "Live", "Completed", "Cancelled"

        [Display(Name = "Home Score")]
        public int HomeTotalScore { get; set; } = 0;

        [Display(Name = "Away Score")]
        public int AwayTotalScore { get; set; } = 0;

        // Foreign Key Relationships
        [ForeignKey("SportId")]
        public Sport? Sport { get; set; }

        [ForeignKey("HomeFacultyId")]
        public Faculty? HomeFaculty { get; set; }

        [ForeignKey("AwayFacultyId")]
        public Faculty? AwayFaculty { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }

        [ForeignKey("ScorekeeperId")]
        public User? Scorekeeper { get; set; }

        // Breakdown scores per set/half
        public ICollection<MatchScore> MatchScores { get; set; } = new List<MatchScore>();
        public ICollection<MatchReminder> MatchReminders { get; set; } = new List<MatchReminder>();
        public ICollection<MatchEvent> Events { get; set; } = new List<MatchEvent>();
    }
}
