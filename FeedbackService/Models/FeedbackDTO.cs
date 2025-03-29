using System.ComponentModel.DataAnnotations;

namespace FeedbackService.Models
{
    public class FeedbackDTO
    {
        public string TargetID { get; set; } 
        public string SenderID { get; set; }  
        public string Comment { get; set; }
        public int Rating { get; set; } 
    }
}
