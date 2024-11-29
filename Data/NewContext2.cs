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

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Message entity
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.Restrict); // Disable cascade delete for Sender

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverID)
                .OnDelete(DeleteBehavior.Restrict); // Disable cascade delete for Receiver
        }
    }
}
