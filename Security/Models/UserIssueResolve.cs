using System.ComponentModel.DataAnnotations;

namespace Security.Models
{
    public class UserIssueResolve
    {
        [Key]
        public long Id {  get; set; }
        public long UserIssueId { get; set; }
        public string Response {get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
