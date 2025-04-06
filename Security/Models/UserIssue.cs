using System.ComponentModel.DataAnnotations;

namespace Security.Models
{
    public class UserIssue
    {
        [Key]
        public long Id { get; set; }
        public string UserId { get; set; }
        public string ReportingUserId { get; set; }
        public string Issue { get; set; }
        public string ImageName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public bool IssueResolve { get; set; }

    }
}
