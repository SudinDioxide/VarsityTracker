using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class VenueFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign Key -> User (Role = "Student")

        [Required]
        public int VenueId { get; set; } // Foreign Key -> Venue

        [Required]
        [StringLength(150)]
        [Display(Name = "Issue Subject")]
        public string Subject { get; set; } = string.Empty; // e.g. "Lighting Defect", "Torn Court Net"

        [Required]
        [StringLength(1000)]
        [Display(Name = "Issue Details / Description")]
        public string Details { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // "Pending", "In Review", "Resolved"

        [Display(Name = "Submitted At")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("VenueId")]
        public Venue? Venue { get; set; }
    }
}

