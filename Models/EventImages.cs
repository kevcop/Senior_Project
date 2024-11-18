using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Senior_Project.Models
{
    public class EventImage
    {
        [Key]
        public int ImageId { get; set; }
        [ForeignKey("Event")]
        public int EventId { get; set; }
        public virtual Event Event { get; set; }
        public byte[] ImageByte { get; set; }
        public string ContentType { get; set; }
        public DateTime Added { get; set; }   

    
    }
}
