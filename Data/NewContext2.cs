using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Senior_Project.Data
{
    public class NewContext2 : DbContext
    {
        public NewContext2(DbContextOptions<NewContext2> options) : base(options) { }

        public DbSet<Register> Register { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventImage> Images { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        // Chat-related tables
        public DbSet<Chat> Chats { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships and constraints for Chat
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(c => c.ChatID); // Primary Key
                entity.HasMany(c => c.Participants) // One-to-many relationship with ChatParticipants
                      .WithOne(p => p.Chat)
                      .HasForeignKey(p => p.ChatID)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete chat and participants

                entity.HasMany(c => c.Messages) // One-to-many relationship with Messages
                      .WithOne(m => m.Chat)
                      .HasForeignKey(m => m.ChatID)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete chat and messages
            });

            // Define relationships and constraints for ChatParticipant
            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                entity.HasKey(cp => cp.ChatParticipantID); // Primary Key
                entity.HasOne(cp => cp.User) // One ChatParticipant -> One User
                      .WithMany()
                      .HasForeignKey(cp => cp.UserID)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict delete of a user to prevent participant deletion
            });

            // Define relationships and constraints for Message
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.MessageID); // Primary Key
                entity.HasOne(m => m.Sender) // One Message -> One Sender
                      .WithMany()
                      .HasForeignKey(m => m.SenderID)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict delete of a sender to avoid message deletion
            });

            // Seed data or additional configurations if needed
        }
    }
}
