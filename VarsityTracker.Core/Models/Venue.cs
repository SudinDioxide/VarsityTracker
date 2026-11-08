using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VarsityTracker.Core.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Venue Name")]
        public string VenueName { get; set; } = string.Empty; // e.g. "Main Stadium", "Court A"

        [Required]
        [StringLength(200)]
        [Display(Name = "Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [StringLength(255)]
        [Display(Name = "Facility Picture / Image URL")]
        public string? ImageUrl { get; set; } // Optional venue photo/image

        // Navigation Properties
        public ICollection<Match> Matches { get; set; } = new List<Match>();
        public ICollection<VenueFeedback> VenueFeedbacks { get; set; } = new List<VenueFeedback>();
    }
}

