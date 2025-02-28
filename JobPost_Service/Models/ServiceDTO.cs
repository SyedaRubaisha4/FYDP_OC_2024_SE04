//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace JobPost_Service.Models
//{
//    public class ServiceDTO
//    {

//        public string ServiceName { get; set; }

     
//        public string ServiceLocation { get; set; }

//        public string HomeAddress { get; set; }

       
//        public DateTime PreferredDate { get; set; }

//        public string PreferredTime { get; set; }

//        //[Range(0, double.MaxValue, ErrorMessage = "Estimated Budget must be a positive number")]
//        public string EstimatedBudget { get; set; }

        
//        public UrgencyLevel UrgencyLevel { get; set; } // Changed to use enum

      
//        public string JobDescription { get; set; }

       
//        public int Experience { get; set; }

//        public string PhoneNumber { get; set; }

       
//        public string Email { get; set; }

    
//        public DateTime DatePosted { get; set; } = DateTime.UtcNow;

//        // Foreign Key for Category
  
//        public int CategoryId { get; set; }

       

//        // Foreign Key for User
    
//        public int UserId { get; set; }


//        // Status of the service
   

//        public ServiceStatus Status { get; set; } = ServiceStatus.Pending; // Default status
//    }

//    // Enum defined inside the Service class
    
//}
