using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VarsityTracker.Core.Models
{
    public class Sport
    {
        [Key]
        public int SportId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Sport Name")]
        public string SportName { get; set; } = string.Empty; // e.g. "Futsal", "Badminton", "Basketball"

        [Required]
        [StringLength(50)]
        [Display(Name = "Scoring Type")]
        public string ScoringType { get; set; } = "Goals"; // "Goals", "Sets", "Points"

        [Display(Name = "Max Periods")]
        [Range(1, 10)]
        public int MaxPeriods { get; set; } = 2; // e.g. 2 (Halves), 3 (Sets), 4 (Quarters)

        // Navigation Properties
        public ICollection<Match> Matches { get; set; } = new List<Match>();
    }
}

