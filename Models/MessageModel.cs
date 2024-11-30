using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    public class Message
    {
        [Key]
        public int MessageID { get; set; } // Primary Key

        [ForeignKey("Chat")]
        public int ChatID { get; set; } // Foreign Key linking to Chat
        public virtual Chat Chat { get; set; } // Navigation property for the chat

        [ForeignKey("Sender")]
        public int SenderID { get; set; } // Foreign Key linking to the user who sent the message
        public virtual Register Sender { get; set; } // Navigation property for the sender user

        [Required]
        public string Content { get; set; } // The actual message content

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Time when the message was sent
    }
}
