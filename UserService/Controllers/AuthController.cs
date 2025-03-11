using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Models;
using UserService.Data;
using UserService.RabbitMQ;
using SharedLibrary;
using Newtonsoft.Json;
using System.Security.Cryptography;
using UserService.Models.JWT;
using UserService.Models.DTOs;
using UserService.Helper;
using System.Reflection;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly IUserProducer _userProducer;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
       

        public AuthController(JwtTokenHelper jwtTokenHelper,UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, IUserProducer userProducer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _userProducer = userProducer;
            _jwtTokenHelper = jwtTokenHelper;
        }
       
        // SignUp API endpoint
        [HttpPost("signup")]     
        public async Task<IActionResult> SignUp([FromForm] SignUpModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            Console.WriteLine(JsonConvert.SerializeObject(model));

          //  Console.WriteLine(model.UserImage);
            // Check if a user with the same phone number already exists
           
            // Create the new user
            var user = new ApplicationUser
            {
               Name = model.Name, 
               Job=model.Job,
               DateofBirth=DateOnly.Parse(model.DateofBirth),
               Password=model.Password,
                Cnic = model.Cnic,
                PhoneNumber = model.PhoneNumber,
                Role = Role.User.ToString(),
                CreatedDate = DateTime.UtcNow,
                Status = Status.Active.ToString(),
                ModifiedDate = null,
                ResetToken = null,
                TokenExpiry = null,
                 City=model.City,
                Experience=null,
                Gender=null,
                 Address =null,
                CertificateImageName=null,
                CnicImageName = null,

            };
            if (model.UserImage != null)
            {
                 user.UserImageName = await SaveFileAsync(model.UserImage, "UserImages");
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            //  var result;// = await _userManager.CreateAsync(user, model.Password);
            if (true)
            {
                // Retrieve the newly created user from the database
                var createdUser = await _context.Users
                                                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

                if (createdUser != null)
                {
                   
                    var publishedUser = new PublishedUser
                    {
                        Id = createdUser.Id,
                        Name = $"{createdUser.Name}",
                        PhoneNumber = createdUser.PhoneNumber
                    };

                    // Publish the data using SendUserAsync
                    await _userProducer.PublishUser(publishedUser);
                    HttpContext.Session.SetString("UserId", user.Id);
                    return Ok(new { message = "User registered successfully!" });
                }

                return BadRequest(new { message = "Error retrieving user after registration." });
            }

            return BadRequest();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            Console.WriteLine(JsonConvert.SerializeObject(model));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
             Console.WriteLine($"Login attempt for PhoneNumber: {model.PhoneNumber}");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber && u.Password==model.Password);
            if (user == null)
            {
                 Console.WriteLine($"No user found with PhoneNumber: {model.PhoneNumber}");
                return BadRequest(new { message = "User not found!" });
            }

            // Check password and sign in the user
            var result = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (result!=null)
            {
                // Retrieve the newly created user from the database
                var createdUser = await _context.Users
                                                .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);

                if (createdUser != null)
                {
                
                    var publishedUser = new PublishedUser
                    {
                        Id = createdUser.Id,
                        Name = $"{createdUser.Name}",
                        PhoneNumber = createdUser.PhoneNumber
                    };

                    // Publish the data using SendUserAsync
                    await _userProducer.PublishUser(publishedUser);
                    HttpContext.Session.SetString("UserId", user.Id);

                }

                // Debugging: Log successful login
                Console.WriteLine($"User logged in successfully with PhoneNumber: {model.PhoneNumber}");
                var accessToken = _jwtTokenHelper.GenerateToken(user);
                var refreshToken = GenerateRefreshToken();
                user.ResetToken = refreshToken;
                user.TokenExpiry = DateTime.UtcNow.AddDays(7);

                await _context.SaveChangesAsync();
                return Ok(new
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    msg = "User created successfully"
                });
            }

            // Debugging: Log failed login attempt
            Console.WriteLine($"Login failed for PhoneNumber: {model.PhoneNumber}");
            return BadRequest(new { message = "Invalid credentials!" });
        }
        [HttpPost("logout")]
        public async Task<bool> Logout(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if(user!=null && user.ResetToken!=null)
            {
                user.ResetToken = null;
                 _context.Users.Update(user);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] RefreshTokenRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetToken == model.RefreshToken);

            if (user == null || user.TokenExpiry < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token.");

            var newAccessToken = _jwtTokenHelper.GenerateToken(user);


            var newRefreshToken = GenerateRefreshToken();
            user.ResetToken = newRefreshToken;
            user.TokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
        private static async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the file", ex);
            }

            return $"/{folderName}/{fileName}";
        }
      
        //[HttpGet("GetUsersById")]
        //public async Task<IActionResult> GetUsersById(string? id)
        //{
        //    ApplicationUser? user;

        //    // Check if ID is null
        //    if (string.IsNullOrEmpty(id))
        //    {
        //        // Get the user ID from session
        //        var userId = HttpContext.Session.GetString("UserId");
        //        if (string.IsNullOrEmpty(userId))
        //        {
        //            return Unauthorized(new { message = "User not logged in!" });
        //        }


        //        user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);
        //    }
        //    else
        //    {

        //        user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id);
        //    }
        //    if (user == null)
        //    {
        //        return NotFound(new { message = "User not found!" });
        //    }
        //    return Ok(new
        //    {
        //        user.Id,
        //        user.FirstName,
        //        user.LastName,
        //        user.PhoneNumber,
        //        user.PasswordHash,
        //        user.Address,
        //        user.Cnic
        //    });
        //}




    }
}
