using System.ComponentModel.DataAnnotations;

namespace JobPost_Service.Models
{
    public class AcceptedJobApplication
    {
        [Key]
        public long Id { get; set; }
        public string UserId { get; set; }
        public string ApplicantId { get; set; }
        public long JobId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
      
    }
}
