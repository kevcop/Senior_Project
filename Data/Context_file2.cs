using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Senior_Project.Models;
using Microsoft.EntityFrameworkCore;

namespace Senior_Project.Data
{
    public class Context_file2 : DbContext

    {
        public Context_file2(DbContextOptions<Context_file2> options) : base(options) { }

        public DbSet<Senior_Project.Models.Event> Events { get; set; }
    }



}
