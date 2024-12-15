using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    /// <summary>
    /// Database for holding event info 
    /// </summary>
    public class Event
    {
        // Unique identifier for an event 
        [Key]
        public int EventID { get; set; }

        // Foreign Key linking to the register database
        [ForeignKey("Register")]
        public int UserID { get; set; }

        // The user assosicated with the event, could be NULL if event is pulled from ticket master 
        public virtual Register ?User { get; set; }
        // The event name 
        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }
        // The description of the event 
        public string Description { get; set; }

        // Date when event will be hosted 
        [Required]
        public DateTime EventDate { get; set; }
        // The location of the event 
        [MaxLength(200)]
        public string Location { get; set; }
        // The category of an event 
        [MaxLength(50)]
        public string Category { get; set; } // E.g., "Sports", "Music", "Theater"
        // When the event was created, more important for user created events
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        // If the event is a private or public one e
        public bool IsPublic { get; set; } = true;
        // Indicate whether a event was created by a user 
        public bool IsUserCreated { get; set; } = false;

        // Collection of images associated with the event 
        public virtual ICollection<EventImage> Images { get; set; } = new List<EventImage>();

        // Ticketmaster id retrieved for ticketmaster fetched events 
        public string ?ExternalEventID { get; set; }
    }
}
