﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Senior_Project.Data
{
    public class Context_file: DbContext

    {
        public Context_file(DbContextOptions<Context_file> options): base(options) { }
    
        public DbSet<Register> Register { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventImage> Images { get; set; }

    }



}
