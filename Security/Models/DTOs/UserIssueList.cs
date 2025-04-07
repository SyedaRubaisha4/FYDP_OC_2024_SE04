namespace Security.Models.DTOs
{
    public class UserIssueList
    {
        public long Id { get; set; }
        public string ReportedUserName { get; set; }
        public string ReportingUserName { get; set; }
        public string Issue { get; set; }
        public string ImageName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public bool IssueResolve { get; set; }
    }
}
