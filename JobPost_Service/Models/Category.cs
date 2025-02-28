using System.ComponentModel.DataAnnotations.Schema;

namespace JobPost_Service.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? CategoryImageName { get; set; }
        public int CategoryJobs { get; set; }

        [NotMapped]
        public IFormFile? CategoryImage { get; set; }

    }
}
