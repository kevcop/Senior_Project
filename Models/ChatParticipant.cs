using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    /// <summary>
    /// Database for tracking the users associated with a chat 
    /// </summary>
    public class ChatParticipant
    {
        // Unique identifer for the user in a chat 
        [Key]
        public int ChatParticipantID { get; set; } 
        // Foreign key linking to the chat database 
        [ForeignKey("Chat")]
        public int ChatID { get; set; }
        // The chat entity assosicated with the chat participant 
        public virtual Chat Chat { get; set; } 
        // Foreign key linking to the register database 
        [ForeignKey("User")]
        public int UserID { get; set; }
        // The user entity assosicated with the chat participant  
        public virtual Register User { get; set; } 
        // Value to determine if a user is a admin of the group
        public bool IsAdmin { get; set; } = false; 
        // Date of the last read message, was meant for group chat implementation( not currently really needed) 
        public DateTime LastReadMessageDate { get; set; } = DateTime.UtcNow; 
    }
}
