namespace FeedbackService.Models
{
    public class FeedbackUpdateDTO
    {
        public string TargetID { get; set; }
        public string SenderID { get; set; }
        public string Comment { get; set; }
        public int Rating { get; set; }
    }
}
