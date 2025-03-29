namespace JobPost_Service.Models
{
    public class UserJob
    {
        public long? Id { get; set; }
        public string? UserId { get; set; }
        public long? JobId { get; set; }
        public string JobsStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }
       
    }
}
