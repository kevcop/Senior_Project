using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    public class ChatParticipant
    {
        [Key]
        public int ChatParticipantID { get; set; } // Primary Key

        [ForeignKey("Chat")]
        public int ChatID { get; set; } // Foreign Key linking to Chat
        public virtual Chat Chat { get; set; } // Navigation property for the chat

        [ForeignKey("User")]
        public int UserID { get; set; } // Foreign Key linking to Register
        public virtual Register User { get; set; } // Navigation property for the user

        public bool IsAdmin { get; set; } = false; // Optional: Indicates if the user is an admin in the group chat

        public DateTime LastReadMessageDate { get; set; } = DateTime.UtcNow; // Tracks when the user last read a message
    }
}
