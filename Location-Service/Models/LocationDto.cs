using System.ComponentModel.DataAnnotations;

namespace Location_Service.Models
{
    public class LocationDto
    {
        [Required]
        public string UserId { get; set; }  // Foreign Key from User Service

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
