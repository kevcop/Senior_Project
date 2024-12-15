using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    /// <summary>
    /// Database that tracks the profile for a user 
    /// </summary>
    public class Profile
    {
        // Unique identifer for a specific profile
        [Key]
        public int Id { get; set; }
        // Foreign key linking to the register database 
        [ForeignKey("Register")]
        public int UserId { get; set; }
        // The register entity associated with the profile 
        public virtual Register User { get; set; }
        // A user's bio 
        [Required]
        [MaxLength(1000)]
        public string Bio { get; set; }
        // A users interests 
        [Required]
        [MaxLength(1000)]
        public string Interests { get; set; }

        // The events the user will be attending in the future
        public ICollection<int> AttendingEvents { get; set; } = new List<int>();
        // The events a user has attended in the past 
        public ICollection<int> PastEvents { get; set; } = new List<int>();
    }
}
