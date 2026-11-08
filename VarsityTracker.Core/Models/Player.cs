using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class Player
    {
        [Key]
        public int PlayerId { get; set; }

        [Required]
        [Display(Name = "Faculty")]
        public int FacultyId { get; set; } // Foreign Key -> Faculty

        [Required]
        [StringLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "Matric Number")]
        public string MatricNo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Position / Role")]
        public string PositionRole { get; set; } = string.Empty; // e.g. "Striker", "Point Guard", "Captain"

        [StringLength(255)]
        [Display(Name = "Photo / Image URL")]
        public string? PhotoUrl { get; set; } // Optional athlete profile picture

        // Navigation Property
        [ForeignKey("FacultyId")]
        public Faculty? Faculty { get; set; }
    }
}

