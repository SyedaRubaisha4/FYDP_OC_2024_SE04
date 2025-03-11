using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPost_Service.Models
{
    public abstract class BasePost
    {
        // Id column with primary key annotation
        [Key]
        public int Id { get; set; }

        // Name column with required validation
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        [Required]
        [StringLength(2000)]
        public string? Description{ get; set; }

        // Location column with required validation
        [Required]
        [StringLength(100)]
        public string? Location { get; set; }

        // Address column with required validation
        [Required]
        [StringLength(200)]
        public string? Address { get; set; }

        // PhoneNumber column with required validation and max length constraint
        [Required]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        // Email column with required validation and max length constraint
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        // Experience column with max length constraint
        [StringLength(500)]
        public string? Experience { get; set; }

        // Salary column with precision and scale annotation for decimal
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal MinSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal MaxSalary { get; set; }

        // PostStatus column with default value and required
        [Required]
        public PostStatus Status { get; set; } = PostStatus.Pending;

        // DatePosted column with a computed value (current date by default)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime DatePosted { get; set; } = DateTime.UtcNow;



        // UserId column for foreign key (foreign key reference)
        [Required]
        public string? UserId { get; set; }

        // CategoryId column for foreign key (foreign key reference)
        [Required]
        public int CategoryId { get; set; }

        // Timing column (for both ServicePost and JobPost)
        [StringLength(50)]
        public string? Timing { get; set; }
        public string Type { get; set; }
    }

    // Enum for PostStatus with annotations
    public enum PostStatus
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }
}
