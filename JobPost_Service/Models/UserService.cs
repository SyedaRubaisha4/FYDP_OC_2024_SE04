namespace JobPost_Service.Models
{
    public class UserService
    {
        public long? Id { get; set; }
        public string? UserId { get; set; }
        public long? ServiceId { get; set; }
        public string ServiceStatus { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? Status { get; set; }

    }
}
