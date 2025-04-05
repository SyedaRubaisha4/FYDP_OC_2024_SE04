namespace NotificationService.Models
{
    public class GetAllJobAcceptedNotificationDTO
    {
        public string JobStatus { get; set; }
        public bool IsSee { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SenderName { get; set; }

        public string NotificationText { get; set; }

    }
}
