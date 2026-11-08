using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class MatchScore
    {
        [Key]
        public int MatchScoreId { get; set; }

        [Required]
        public int MatchId { get; set; } // Foreign Key -> Match

        [Display(Name = "Period / Set / Half")]
        public int PeriodNumber { get; set; } = 1; // 1 (Set 1 / 1st Half), 2, 3, etc.

        [Display(Name = "Home Points")]
        public int HomePoints { get; set; } = 0;

        [Display(Name = "Away Points")]
        public int AwayPoints { get; set; } = 0;

        [ForeignKey("MatchId")]
        public Match? Match { get; set; }
    }
}

