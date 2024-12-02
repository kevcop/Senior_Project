namespace Senior_Project.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }
        public int EventId { get; set; } // Foreign key to associate with an event
        public string Title { get; set; } // Discussion topic or title
        public DateTime CreatedAt { get; set; }
        public List<Message> Messages { get; set; } // Messages in the discussion
        public List<DiscussionParticipant> Participants { get; set; } // Users in the discussion
    }

    public class DiscussionParticipant
    {
        public int Id { get; set; }
        public int DiscussionId { get; set; } // Foreign Key
        public Discussion Discussion { get; set; } // Navigation Property
        public int UserId { get; set; } // Foreign Key for User
        public Register User { get; set; } // Navigation Property for User
    }

}
