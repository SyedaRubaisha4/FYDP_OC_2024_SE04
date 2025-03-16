using Microsoft.EntityFrameworkCore;
using FeedbackService.Models;
using System.Collections.Generic;

namespace FeedbackService.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }
        public DbSet<Feedback> Feedbacks { get; set; }
    }
}
