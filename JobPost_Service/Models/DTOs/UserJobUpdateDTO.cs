namespace JobPost_Service.Models.DTOs
{
    public class UserJobUpdateDTO
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public long? JobId { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
