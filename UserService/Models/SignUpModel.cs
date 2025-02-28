namespace UserService.Models
{
    public class SignUpModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public IFormFile UserImage { get; set; }
        public string Job { get; set; }
        public string DateofBirth { get; set; }
        public string Cnic { get; set; }
        public string City { get; set; }
    }
}
