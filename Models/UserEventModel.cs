using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Senior_Project.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        // Foreign Key linking to the Register table
        [ForeignKey("Register")]
        public int UserID { get; set; }

        // Navigation property for accessing the user who created the event
        public virtual Register User { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } // E.g., "Sports", "Music", "Theater"

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsPublic { get; set; } = true;

        public bool IsUserCreated { get; set; } = false; // True for user-created events, false for popular events

        public virtual ICollection<EventImage> Images { get; set; }


    }
}
