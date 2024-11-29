using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    public class Message
    {
        [Key]
        public int MessageID { get; set; } // Primary Key

        [ForeignKey("Sender")]
        public int SenderID { get; set; }
        public virtual Register Sender { get; set; } // Navigation property for the sender user

        [ForeignKey("Receiver")]
        public int ReceiverID { get; set; }
        public virtual Register Receiver { get; set; } // Navigation property for the receiver user

        [Required]
        [MaxLength(1000)] // Limit message length to 1000 characters
        public string Content { get; set; } // The actual message content

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Time when the message was sent

        public bool IsRead { get; set; } = false; // Indicates if the message has been read by the receiver
    }
}
