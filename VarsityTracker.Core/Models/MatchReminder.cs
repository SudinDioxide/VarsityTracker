using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VarsityTracker.Core.Models
{
    public class MatchReminder
    {
        [Key]
        public int ReminderId { get; set; }

        [Required]
        public int UserId { get; set; } // Foreign Key -> User (Role = "Student")

        [Required]
        public int MatchId { get; set; } // Foreign Key -> Match

        [Display(Name = "Alert Lead Time (Minutes)")]
        public int LeadTimeMinutes { get; set; } = 30; // e.g. 15, 30, 60 minutes before match

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("MatchId")]
        public Match? Match { get; set; }
    }
}

