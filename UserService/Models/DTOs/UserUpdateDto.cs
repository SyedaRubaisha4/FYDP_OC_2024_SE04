using System.ComponentModel.DataAnnotations;

namespace UserService.Models.DTOs
{
    public class UserUpdateDto
    {
        public string Name { get; set; }

       // [MaxLength(11)]
        public string PhoneNumber { get; set; }

      //  [MaxLength(10)]
        public string Password { get; set; }

     //   [MaxLength(150)]
        public string Address { get; set; }
//
      //  [MaxLength(13)]
        public string Cnic { get; set; }

        public string Gender { get; set; }
        public string City { get; set; }
      //  public DateOnly DateofBirth { get; set; }

        public string Experience { get; set; }  // ✅ Added from JSON
        public string Job { get; set; }  
      
        public IFormFile CnicImageName { get; set; }  
        public IFormFile CertificateImageName { get; set; }  // ✅ JSON field
        public IFormFile UserImageName { get; set; }  // ✅ JSON field
    }
}
