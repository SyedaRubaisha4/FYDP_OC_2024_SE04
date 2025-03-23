using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Helper;
using UserService.Models;
using UserService.Models.DTOs;
using UserService.Models.AuthUser;
using SharedLibrary;
using UserService.RabbitMQ;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUserProducer _userProducer;
        public UsersController(ApplicationDbContext context, IHttpContextAccessor contextAccessor, IUserProducer userProducer)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _userProducer = userProducer;
        }
        [HttpGet("GetAllUsers")]
       // [Authorize]
        public async Task<List<ApplicationUser>> GetUser()
        {
            var users = await _context.Users
                                      .Where(x => x.Status == Status.Active.ToString())
                                      .ToListAsync();
            return users;
        }

       // [HttpGet("GetUsersById/{id}")]//hhrkii  <Button title="Update Profile" onPress={() => navigation.navigate('UpdateProfile', { userData: data })} />
       //// [Authorize]

       // //ghfghff
       // public async Task<ActionResult<ApplicationUser>> GetUser(string id)
       // {
       //     var user = await _context.Users
       //         .Where(x => x.Status == Status.Active.ToString() && x.Id == id)
       //         .FirstOrDefaultAsync();
       //     var publishedUser = new PublishedUser
       //     {
       //         Id = user.Id,
       //         Name = $"{user.Name}",
       //         PhoneNumber = user.PhoneNumber,
       //         Experience=user.Experience,
       //         UserImage=user.UserImageName,
       //         Job=user.Job,
       //         City=user.Job,
       //         Role=user.Role,
       //     };
       //     await _userProducer.PublishUser(publishedUser);
       //     if (user == null)
       //     {
       //         return NotFound();
       //     }
       //     return Ok(user);
       // }

        [HttpPut("UpdateUser/{id}")]
        //[Authorize]
        public async Task<ActionResult<ApplicationUser>> UpdateUser(string id, [FromForm] UserUpdateDto user)
        {
            Console.WriteLine("Ma phnch gaya yaha \n \n \n\n tk !!!!!!!!!!!!!!!!!!!!!");
            var UpdateUser = await _context.Users
                .Where(x => x.Id == id && x.Status == Status.Active.ToString())
                .FirstOrDefaultAsync();
            if (UpdateUser == null)
            {
                return NotFound("User not found or inactive.");
            }
            // Update fields
            UpdateUser.Name = user.Name;
        
            UpdateUser.PhoneNumber = user.PhoneNumber;
            UpdateUser.Address = user.Address;
            UpdateUser.Password = user.Password;
            UpdateUser.Cnic = user.Cnic;
            UpdateUser.City = user.City;
            UpdateUser.Job = user.Job;
          
            UpdateUser.Experience = user.Experience;
           UpdateUser.DateofBirth = user.DateofBirth;
            UpdateUser.Gender = user.Gender;
            if (user.UserImageName != null)
            {
                DeleteFile(UpdateUser.UserImageName);
                 UpdateUser.UserImageName = await SaveFileAsync(user.UserImageName, "UserImages");
            }
           

            if (user.CertificateImageName != null)
            {
                DeleteFile(UpdateUser.CertificateImageName);
                UpdateUser.CertificateImageName = await SaveFileAsync(user.CertificateImageName, "UserCertificateImages");
            }

            if (user.CnicImageName != null)
            {
                DeleteFile(UpdateUser.CnicImageName);
                 UpdateUser.CnicImageName = await SaveFileAsync(user.CnicImageName, "UserCnicImages");
            }

            UpdateUser.ModifiedDate = DateTime.UtcNow;

            // **Ensure EntityState is Modified**
            _context.Entry(UpdateUser).State = EntityState.Modified;

            // **Save changes to DB**
            await _context.SaveChangesAsync();

            return Ok(UpdateUser);
        }
        [HttpGet("GetUserData")]
        public async Task<UserGetDto> GetUserData()
        {
            
            var authUser = new AuthUser(_contextAccessor.HttpContext.Request);
            var user = await _context.Users.Where(x => x.Id == authUser.Id).FirstOrDefaultAsync();
            var User = new UserGetDto
            {
                UserImageName = user.UserImageName,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Password = user.Password,
                Address = user.Address,
                Cnic = user.Cnic,
                Gender = user.Gender,
                City = user.City,
                CnicImageName = user.CnicImageName,
                CertificateImageName = user.CertificateImageName,
                DateofBirth = user.DateofBirth,
                Experience = user.Experience,
                Job = user.Job,                
            };


            return User;
        }   
        [HttpPost("AddUser")]
       // [Authorize]
        public async Task<ActionResult<ApplicationUser>> AddUser(UserCreateDto UserCreateDto)
        {
            var user = new ApplicationUser
            {
                Name = UserCreateDto.Name,
                PhoneNumber = UserCreateDto.PhoneNumber,
                Password = UserCreateDto.Password,
                Address = UserCreateDto.Address,
                Cnic = UserCreateDto.Cnic,
                Gender = UserCreateDto.Gender,
                City = UserCreateDto.City,
                DateofBirth = UserCreateDto.DateofBirth,
            };
            if(UserCreateDto.UserImage!=null)
            {
               user.UserImageName=await SaveFileAsync(UserCreateDto.UserImage, "UserImages");
            }
            else
            {
                user.UserImageName = null;
            }
            if (UserCreateDto.CnicImage != null)
            {
                user.CnicImageName = await SaveFileAsync(UserCreateDto.CnicImage, "UserCnicImages");
            }
            else
            {
                user.CnicImageName = null;
            }
            if (UserCreateDto.CertificateImage != null)
            {
                user.CertificateImageName = await SaveFileAsync(UserCreateDto.CertificateImage, "UserCertificateImages");
            }
            else
            {
                user.CertificateImageName = null;
            }
            user.Role = Role.User.ToString();
            user.CreatedDate=DateTime.UtcNow;
            user.Status = Status.Active.ToString();
            user.ModifiedDate=null;
            user.ResetToken = null;
            user.TokenExpiry = null;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }
 
       // [Authorize]
        [HttpDelete("DeleteUser{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Status = Status.Blocked.ToString();
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
       // [Authorize]
        [HttpGet("Count")]
        public async Task<IActionResult> GetUsersAllCount()
        {
            var users= await _context.Users.Where(x=>x.Status==Status.Active.ToString()).CountAsync();
            return Ok(users);
        }
       // [Authorize]
        [HttpGet("GetUsersWeeklyCount")]
        public async Task<IActionResult> GetUsersWeeklyCount()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
             var usersThisWeek = await _context.Users
                .Where(x => x.Status == Status.Active.ToString() && x.CreatedDate >= startOfWeek)
                .CountAsync(); 
            return Ok(usersThisWeek);
        }
      //  [Authorize]
        [HttpGet("GetUsersMonthlyCount")]
        public async Task<IActionResult> GetUsersMonthlyCount()
        {
            var now = DateTime.UtcNow; var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisMonth = await _context.Users
             .Where(x => x.Status == Status.Active.ToString() && x.CreatedDate >= startOfMonth)
             .CountAsync();
            return Ok(usersThisMonth);
        }

        [HttpGet("GetUsersFromJob/{job}")]
        public async Task<IActionResult> GetUsersFromJob(string Job)
        {
            var getUsers = await _context.Users.Where(x => x.Job == Job && x.Status==Status.Active.ToString()).ToListAsync();
            return Ok(getUsers);
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
        private static void DeleteFile(string filePath)
        {
            if (filePath != null)
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }
        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
