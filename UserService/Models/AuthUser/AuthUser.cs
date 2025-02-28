using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserService.Models.AuthUser
{
    public class AuthUser
    {
        public string Id { get; set; }
        public string? FirstName { get; set; }
        public string? PhoneNumber { get; set; }


        public AuthUser(HttpRequest request)
        {
            var token = request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("Token is missing in the request.");
            }

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            Id = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value;
            PhoneNumber = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "PhoneNumber")?.Value;
            FirstName = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "FirstName")?.Value;
        }


    }
}
