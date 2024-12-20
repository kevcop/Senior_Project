using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Senior_Project.Data
{
    /// <summary>
    /// Database context file, provides access to database tables 
    /// </summary>
    public class NewContext2 : DbContext
    {
        /// <summary>
        /// Initialize instance of the class 
        /// </summary>
        /// <param name="options"> Options for configuring the database context</param>

        public NewContext2(DbContextOptions<NewContext2> options) : base(options) { }
        // Register table
        public DbSet<Register> Register { get; set; }
        // Events table 
        public DbSet<Event> Events { get; set; }
        // Images table
        public DbSet<EventImage> Images { get; set; }
        // Profiles table
        public DbSet<Profile> Profiles { get; set; }

        // Chat table
        public DbSet<Chat> Chats { get; set; }
        // Participants of a chat table 
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        // Messages within a chat table
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chat table configuration
            modelBuilder.Entity<Chat>(entity =>
            {
                // Primary Key 
                entity.HasKey(c => c.ChatID); 
                // One to many relationship between chat and chatParticipants
                entity.HasMany(c => c.Participants) 
                      .WithOne(p => p.Chat)
                      .HasForeignKey(p => p.ChatID)
                      // Cascade delete to remove participants when a chat is deleted 
                      .OnDelete(DeleteBehavior.Cascade);
                // One to many relationship between chat and messages
                entity.HasMany(c => c.Messages)
                      .WithOne(m => m.Chat)
                      .HasForeignKey(m => m.ChatID)
                      // Cascade delete to remove messages when a chat is deleted 
                      .OnDelete(DeleteBehavior.Cascade); 
                // Optional relationship between chat and ecent 
                entity.HasOne(c => c.Event) 
                      .WithMany()
                      .HasForeignKey(c => c.EventId)
                      // Prevent deletion of events when chat is deleted 
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            // Chat participant table configuration 
            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                // Set primary key 
                entity.HasKey(cp => cp.ChatParticipantID); 
                // One to one relationship between chat particpant and user 
                entity.HasOne(cp => cp.User) 
                      .WithMany()
                      .HasForeignKey(cp => cp.UserID)
                      // Restrict user deletion
                      .OnDelete(DeleteBehavior.Restrict); 
            });

            // Message table configuration
            modelBuilder.Entity<Message>(entity =>
            {
                // Primary key 
                entity.HasKey(m => m.MessageID); 
                // One to one relationship between message and sender
                entity.HasOne(m => m.Sender) 
                      .WithMany()
                      .HasForeignKey(m => m.SenderID)
                      // Restrict deletion of sender
                      .OnDelete(DeleteBehavior.Restrict); 
            });
        }

    }
}
