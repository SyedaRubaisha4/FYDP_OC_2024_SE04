using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace SharedLibrary
{
    public class PublishedUser
    {
        [Key]
        public string? Id { get; set; }
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public string? City { get; set; }
        public string? Experience { get; set; }
        public string? Job { get; set; }
        public string? UserImage { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }

    }
}
