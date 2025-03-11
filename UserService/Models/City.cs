using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class City
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
      public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
