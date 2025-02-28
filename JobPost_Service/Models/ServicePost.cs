
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

namespace JobPost_Service.Models

{
    public class ServicePost : BasePost
    {
        public DateTime PreferredDate { get; set; }
        public UrgencyLevel UrgencyLevel { get; set; }
    }

    public enum UrgencyLevel
    {
       Normal,
       Emergency,
       
    }
}