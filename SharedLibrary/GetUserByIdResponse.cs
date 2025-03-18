using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{
    public class GetUserByIdResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Experience { get; set; }
        public string UserImage { get; set; }
        public string Job { get; set; }
        public string City { get; set; }
        public string Role { get; set; }
    }
}
