using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VarsityTracker.Core.Models
{
    public class Faculty
    {
        [Key]
        public int FacultyId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Faculty Code")]
        public string FacultyCode { get; set; } = string.Empty; // e.g., "FSKTM", "FKEE"

        [Required]
        [StringLength(150)]
        [Display(Name = "Faculty Name")]
        public string FacultyName { get; set; } = string.Empty; // Full Title

        [StringLength(100)]
        [Display(Name = "Sports Manager")]
        public string ManagerName { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Player> Players { get; set; } = new List<Player>();
        public ICollection<User> FacultyUsers { get; set; } = new List<User>();
    }
}

