using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models.DTOs
{
    public class UserCreateDto
    {
        public string?Name { get; set; }
   
        [MaxLength(11)]
        public string? PhoneNumber { get; set; }
        [MaxLength(10)]
        public string? Password { get; set; }
        [MaxLength(150)]
        public string? Address { get; set; }
        [MaxLength(13)]
        public string? Cnic { get; set; }
        public string? Gender { get; set; }
        public string? City { get; set; }
        public DateOnly DateofBirth { get; set; }
        public IFormFile CnicImage { get; set; }
        public IFormFile? CertificateImage { get; set; }     
        public IFormFile? UserImage { get; set; }
    }
}
