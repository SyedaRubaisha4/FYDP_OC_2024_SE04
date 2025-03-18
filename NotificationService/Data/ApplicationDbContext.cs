using Microsoft.EntityFrameworkCore;
using NotificationService.Models;
using System.Collections.Generic;
namespace NotificationService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {
            
        }
        public DbSet<AcceptedJobNotifcation> AcceptedJobNotifcation { get; set; }
    }
}
