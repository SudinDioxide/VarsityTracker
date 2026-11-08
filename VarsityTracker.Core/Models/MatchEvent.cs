using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class MatchEvent
    {
        [Key]
        public int MatchEventId { get; set; }

        [Required]
        public int MatchId { get; set; }

        public int? FacultyId { get; set; }

        [Required]
        [StringLength(40)]
        public string EventType { get; set; } = "Goal"; // Goal, Point, YellowCard, RedCard, PeriodStart, PeriodEnd, Foul, Substitution, Highlight

        [Required]
        [StringLength(20)]
        public string Minute { get; set; } = "1'"; // e.g. "12'", "45+2'", "Set 1", "HT", "FT"

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        [StringLength(100)]
        public string? PlayerName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Key Relationships
        [ForeignKey("MatchId")]
        public Match? Match { get; set; }

        [ForeignKey("FacultyId")]
        public Faculty? Faculty { get; set; }
    }
}

