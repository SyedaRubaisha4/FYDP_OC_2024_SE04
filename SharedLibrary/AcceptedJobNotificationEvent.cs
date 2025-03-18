using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class AcceptedJobNotificationEvent
    {
        public string UserId { get; set; }
        public string ApplicantId { get; set; }
        public long JobId { get; set; }
        public string JobStatus { get; set; }
        
    }

}
