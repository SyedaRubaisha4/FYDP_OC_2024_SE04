using System.ComponentModel.DataAnnotations;

namespace JobPost_Service.Models
{
    public class AcceptedServiceApplication
    {
        [Key]
        public long Id { get; set; }
        public string UserId { get; set; }
        public string ApplicantId { get; set; }
        public long ServiceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
       

    }
}
