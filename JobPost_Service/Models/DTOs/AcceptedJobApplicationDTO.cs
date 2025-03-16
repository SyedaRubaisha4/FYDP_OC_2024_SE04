namespace JobPost_Service.Models.DTOs
{
    public class AcceptedJobApplicationDTO
    {
        public string UserId { get; set; }
        public string ApplicantId { get; set; }
        public long JobId { get; set; }
        public string JobsStatus { get; set; }

    }
}
