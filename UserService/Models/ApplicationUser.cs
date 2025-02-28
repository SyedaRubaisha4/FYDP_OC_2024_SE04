using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        [MaxLength(11)]
        //signup
        public string? PhoneNumber { get; set; }
        [MaxLength(10)]
        //signup
        public string? Password { get; set; }
        [MaxLength(150)]
        public string? Address { get; set; }
        [MaxLength(13)]
        //signup
        public string? Cnic { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public string? Gender { get; set; }
        //signup
        public string? City { get; set; }
        public string? CnicImageName { get; set; }
        public string? UserImageName { get; set; }
        public string? CertificateImageName { get; set; }
        //signup
        public DateOnly DateofBirth { get; set; }
        public string? ResetToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public string? Experience { get; set; }
        public string? Job { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        [NotMapped]
        public IFormFile CnicImage { get; set; }
        [NotMapped]
        public IFormFile? CertificateImage { get; set; }
        [NotMapped]
        //signup
        public IFormFile? UserImage { get; set; }

    }
}
