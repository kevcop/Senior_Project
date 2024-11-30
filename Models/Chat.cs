using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Senior_Project.Models
{
    public class Chat
    {
        [Key]
        public int ChatID { get; set; } // Primary Key

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Timestamp for chat creation

        public string ChatName { get; set; } // Optional: Name for group chats

        public DateTime LastMessageDate { get; set; } // Timestamp of the last message sent in the chat

        public bool IsGroupChat { get; set; } = false; // Indicates if it's a group chat

        // Navigation property for related participants
        public virtual ICollection<ChatParticipant> Participants { get; set; }

        // Navigation property for related messages
        public virtual ICollection<Message> Messages { get; set; }
    }
}
