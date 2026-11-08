using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Matric / Staff ID")]
        public string MatricNo { get; set; } = string.Empty; // Unique ID (Student / Official / Admin)

        [Required]
        [StringLength(120)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(120)]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        [Display(Name = "System Role")]
        public string Role { get; set; } = "Student"; // "Admin", "Scorekeeper", "Student"

        // Optional link if user belongs to a specific faculty
        [Display(Name = "Primary Faculty")]
        public int? FacultyId { get; set; }

        [ForeignKey("FacultyId")]
        public Faculty? Faculty { get; set; }

        // Navigation Properties
        [InverseProperty("Scorekeeper")]
        public ICollection<Match> AssignedMatches { get; set; } = new List<Match>();

        public ICollection<MatchReminder> MatchReminders { get; set; } = new List<MatchReminder>();

        public ICollection<VenueFeedback> VenueFeedbacks { get; set; } = new List<VenueFeedback>();
    }
}

