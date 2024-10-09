using System;
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
    
        public DbSet<Senior_Project.Models.Register> Register { get; set; }
    }



}
