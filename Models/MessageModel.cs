using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    /// <summary>
    ///  Database for holding all messages 
    /// </summary>
    public class Message
    {
        // Unique identifier for a message 
        [Key]
        public int MessageID { get; set; } 
        // Foreign key linking a message to a chat 

        [ForeignKey("Chat")]
        public int ChatID { get; set; } 
        // The chat entity assosicated with a message 
        public virtual Chat Chat { get; set; } 
        // Foreign key linking to a user id in the register table 
        [ForeignKey("Sender")]
        public int SenderID { get; set; } 
        // The user associated with the message 
        public virtual Register Sender { get; set; } 
        // The message content 
        [Required]
        public string Content { get; set; } 
        // Time message was sent 
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; 
    }
}
