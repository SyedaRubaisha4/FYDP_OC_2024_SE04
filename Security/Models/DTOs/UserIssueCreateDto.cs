namespace Security.Models.DTOs
{
    public class UserIssueCreateDto
    {
        public string UserId { get; set; }
        public string ReportingUserId { get; set; }
        public string Message { get; set; }
        public IFormFile? Image { get; set; }
    }
}
