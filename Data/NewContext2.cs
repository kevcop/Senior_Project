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

            // Chat configuration
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

                entity.HasOne(c => c.Event) // One-to-one or many-to-one with Event
                      .WithMany()
                      .HasForeignKey(c => c.EventId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent event deletion from cascading
            });

            // ChatParticipant configuration
            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                entity.HasKey(cp => cp.ChatParticipantID); // Primary Key
                entity.HasOne(cp => cp.User) // One ChatParticipant -> One User
                      .WithMany()
                      .HasForeignKey(cp => cp.UserID)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict delete of a user to prevent participant deletion
            });

            // Message configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.MessageID); // Primary Key
                entity.HasOne(m => m.Sender) // One Message -> One Sender
                      .WithMany()
                      .HasForeignKey(m => m.SenderID)
                      .OnDelete(DeleteBehavior.Restrict); // Restrict delete of a sender to avoid message deletion
            });
        }

    }
}
