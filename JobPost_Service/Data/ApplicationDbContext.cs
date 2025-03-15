using Microsoft.EntityFrameworkCore;
using JobPost_Service.Models;
using SharedLibrary;

namespace JobPost_Service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ServicePost> ServicePosts { get; set; }
        public DbSet<JobPost> JobPosts { get; set; }
     //   public DbSet<PublishedUser> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<UserJob> UserJob { get; set; }
        public DbSet<UserService> UserService { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define foreign key relationships
            modelBuilder.Entity<ServicePost>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(sp => sp.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JobPost>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(jp => jp.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Remove UserId as a foreign key
            modelBuilder.Entity<ServicePost>()
                .Property(sp => sp.UserId)
                .IsRequired(); // Keep it as a simple column (optional)

            modelBuilder.Entity<JobPost>()
                .Property(jp => jp.UserId)
                .IsRequired(); // Keep it as a simple column (optional)
        }
    }
}
