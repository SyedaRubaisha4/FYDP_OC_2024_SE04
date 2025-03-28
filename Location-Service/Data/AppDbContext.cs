using Location_Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Location_Service.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Location> Locations { get; set; }
    }
}
