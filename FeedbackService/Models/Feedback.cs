using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FeedbackService.Models
{
    public class Feedback
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public string? TargetID { get; set; }  // ID of the user or job post the feedback is for

        [Required]
        public string? SenderID { get; set; }  // ID of the user or job who post the feedback 
        public string? Comment { get; set; }
        public int? Rating { get; set; }  // Optional rating (1-5)
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }  // Timestamp for when feedback is modified
        [Required]
        public string? Status { get; set; } // e.g., "Active", "Deleted"
    }

}
