using System.ComponentModel.DataAnnotations;

namespace Location_Service.Models
{
    public class Location
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // Foreign Key from User Service

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
