using Microsoft.EntityFrameworkCore;
using Security.Models;
namespace Security.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
        {

        }
        public DbSet<UserIssue> UserIssue { get; set; }
        public DbSet<UserIssueResolve> userIssueResolves { get; set; }
    }
}
