using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.Models.JWT
{
    public class JwtTokenHelper
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenHelper(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }
        public string GenerateToken(ApplicationUser user)
        {
            var issuer = SharedUtilityConnection.AppConfiguration["JWT:ValidIssuer"];
            var sceretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SharedUtilityConnection.AppConfiguration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(sceretKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                           issuer: issuer,
                           audience: issuer,
                           claims: MapClaims(user),
                           expires: DateTime.Now.AddDays(1),
                           signingCredentials: signingCredentials);

            var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
            return jwttoken;
        }
        public ApplicationUser GetUserFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            // Validate if the token is in correct format
            if (!handler.CanReadToken(token))
            {
                return null;
            }

            var jwtToken = handler.ReadJwtToken(token);

            // Extract claims
            var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            var phoneClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "PhoneNumber")?.Value;

            if (string.IsNullOrEmpty(idClaim) || string.IsNullOrEmpty(phoneClaim))
            {
                return null;
            }

            // Create and return User object with extracted claims
            return new ApplicationUser
            {
                Id = idClaim,
                PhoneNumber = phoneClaim
            };
        }

        List<Claim> MapClaims(ApplicationUser userDto)
        {
            List<Claim> claim = new List<Claim>();
            claim.Add(new Claim("Id", userDto.Id.ToString()));
            claim.Add(new Claim("PhoneNumber", userDto.PhoneNumber));
            return claim;
        }
    }
}
