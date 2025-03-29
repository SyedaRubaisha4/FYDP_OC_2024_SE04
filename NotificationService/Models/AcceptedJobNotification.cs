using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models
{
    public class AcceptedJobNotifcation
    {
        [Key]
        public long Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public long UserJobId { get; set; }
        public string JobStatus { get; set; }
        public bool IsSee {  get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }

    }
}
