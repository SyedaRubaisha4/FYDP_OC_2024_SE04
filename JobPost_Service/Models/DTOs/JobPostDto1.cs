using System.ComponentModel.DataAnnotations;

namespace JobPost_Service.Models.DTOs
{
    public class JobPostDto1
    {
        public string? JobType { get; set; } // Example: Full-time, Part-time
        public string? WorkplaceType { get; set; } // Example: Remote, On-site, Hybrid
        public List<string> Skills { get; set; }
        public string? CompanyName { get; set; }
        public string? Name { get; set; }
        [Required]
        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string? Location { get; set; }

        [Required]
        [StringLength(200)]
        public string? Address { get; set; }

        [Required]
        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Experience { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal MinSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal MaxSalary { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime DatePosted { get; set; }

        [Required]
        public string? UserId { get; set; }

        // CategoryId column for foreign key (foreign key reference)
        [Required]
        public int CategoryId { get; set; }


        [StringLength(50)]
        public string? Timing { get; set; }
        public string Type { get; set; }
        public string JobsStatus { get; set; }
        public string AppliedDate { get; set; }
    }
}
