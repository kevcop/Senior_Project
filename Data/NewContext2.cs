using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;
namespace Senior_Project.Data
{
    public class NewContext2:DbContext
    
        
    {
        public NewContext2(DbContextOptions<NewContext2> options) : base(options) { }

        public DbSet<Register> Register { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventImage> Images { get; set; }

        public DbSet<Profile> Profiles { get; set; }

    }
}
