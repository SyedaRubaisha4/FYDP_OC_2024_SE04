//using System.ComponentModel;
//using System.ComponentModel.DataAnnotations;

//namespace JobPost_Service.Models
//{
//    public class JobPost
//    {
//        public int Id { get; set; }

//        [Required(ErrorMessage = "Job title is required.")]
//        public string Title { get; set; }

//        [Required(ErrorMessage = "Location is required.")]
//        public string Location { get; set; }

//        [Required(ErrorMessage = "Job type is required.")]
//        public string JobType { get; set; } // Example: Full-time, Part-time

//        [Required(ErrorMessage = "Workplace type is required.")]
//        public string WorkplaceType { get; set; } // Example: Remote, On-site, Hybrid

//        [Required(ErrorMessage = "Experience is required.")]
//        public string Experience { get; set; } // Example: "1-3 years" or "No experience required"

//        [Required(ErrorMessage = "Salary range is required.")]
//        public string SalaryRange { get; set; } // Example: "50,000 - 70,000"

//        [Required(ErrorMessage = "Job description is required.")]
//        public string JobDescription { get; set; }

//        [Required(ErrorMessage = "Skills are required.")]
//        public List<string> Skills { get; set; } = new List<string>();

//        // Company details
//        [Required(ErrorMessage = "Company name is required.")]
//        public string CompanyName { get; set; }

//        [Required(ErrorMessage = "Company address is required.")]
//        public string CompanyAddress { get; set; }

//        [Required(ErrorMessage = "Company phone number is required.")]
//        [Phone(ErrorMessage = "Invalid phone number format.")]
//        public string CompanyPhoneNumber { get; set; }

//        // New fields
//        // Last updated date

//        [Required(ErrorMessage = "Date posted is required.")]
//        public DateTime DatePosted { get; set; }

//        [Required]
//        [DefaultValue("Active")] // Default value for the Status field
//        public string Status { get; set; } = "Active";
//    }
//}




using JobPost_Service.Models;

public class JobPost : BasePost
{
    public string? JobType { get; set; } // Example: Full-time, Part-time
    public string? WorkplaceType { get; set; } // Example: Remote, On-site, Hybrid
    public List<string> Skills { get; set; } = new List<string>();
    public string? CompanyName { get; set; } // Company details
}
