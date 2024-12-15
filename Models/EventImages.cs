using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Senior_Project.Models
{
    /// <summary>
    /// Event related images database
    /// </summary>
    public class EventImage
    {
        // Unique identifer for an image 
        [Key]
        public int ImageId { get; set; }
        // Foreign key linking to an event 
        [ForeignKey("Event")]
        public int EventId { get; set; }
        // The event entity assosicated with the image 
        public virtual Event Event { get; set; }
        // The image storage location 
        public string FilePath { get; set; } 
        // The file extension of the image ( JPEG,PNG)
        public string ContentType { get; set; }
        // Date of when image was added 
        public DateTime Added { get; set; }
    }
}
