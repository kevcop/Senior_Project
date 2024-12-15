using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Senior_Project.Models
{
    /// <summary>
    /// The chat database
    /// </summary>
    public class Chat
{
    // Unique identifier for a chat 
    [Key]
    public int ChatID { get; set; } 

    // The date when the chat was created 
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; 
    // The name for a chat
    public string ChatName { get; set; } 
    // Date of the latest sent message 
    public DateTime LastMessageDate { get; set; }
    // Identifer for group chats 
    public bool IsGroupChat { get; set; } = false; 
    // Identifer for discussions for events 
    public bool IsDiscussion { get; set; } = false; 
    // Foreign key linking to an event 
    public int? EventId { get; set; } 
    // The event entity assosicated with the chat 
    public virtual Event Event { get; set; } 

    // The collection of participants in a chat 
    public virtual ICollection<ChatParticipant> Participants { get; set; }

    // The collection of messages for a chat 
    public virtual ICollection<Message> Messages { get; set; }
}

}
