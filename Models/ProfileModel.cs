using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Register")]
        public int UserId { get; set; }
        public virtual Register User { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Bio { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Interests { get; set; }

        // Many-to-many relationship for attending events
        public ICollection<int> AttendingEvents { get; set; } = new List<int>();
        public ICollection<int> PastEvents { get; set; } = new List<int>();
    }
}
