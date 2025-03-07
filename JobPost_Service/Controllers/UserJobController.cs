using JobPost_Service.Data;
using JobPost_Service.Helper;
using JobPost_Service.Models;
using JobPost_Service.Models.DTOs;
using JobPost_Service.RabbitMQ;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;

namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserJobController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly UserRequestProducer _userRequestProducer;

        public UserJobController(ApplicationDbContext context, IMemoryCache memoryCache, UserRequestProducer UserRequestProducer)
        {
            _context = context;
            _memoryCache = memoryCache;
            _userRequestProducer= UserRequestProducer;
        }

        [HttpPost("CreateUserjob")]
        public async Task<ActionResult<UserJob>> CreateUserJob(UserJobCreateDTO UserJobCreateDTO)
        {
            if (!_memoryCache.TryGetValue("User", out PublishedUser user) || user == null)
            {
                return BadRequest("No user found in cache.");
            }
            var UserJob = new UserJob
            {
                UserId = user.Id, // Cache se UserId le li
                JobId = UserJobCreateDTO.JobId,
                CreatedDate = DateTime.Now,
                Status = Status.Active.ToString(),
                ModifiedDate = null
            };

            if (UserJob.JobId != null) // Sirf JobId check karni hai
            {
                _context.UserJob.Add(UserJob);
                await _context.SaveChangesAsync();
                return Ok(UserJob);
            }

            return BadRequest("JobId is null");
        }

        [HttpPut("UpdateUserjob/{id}")]
        public async Task<ActionResult<UserJob>> UpdateUserJob(UserJobUpdateDTO UserJobUpdateDTO)
        {

            var userjob = await _context.UserJob.FindAsync(UserJobUpdateDTO.Id);
            if (UserJobUpdateDTO.Id != null)
            {


                userjob.UserId = UserJobUpdateDTO.UserId;
                userjob.JobId = UserJobUpdateDTO.JobId;
                //  userjob.CreatedDate = UserJobUpdateDTO.CreatedDate;

                userjob.ModifiedDate = DateTime.Now;

                _context.UserJob.Update(userjob);
                _context.SaveChanges();
                return Ok(userjob);

            }
            else
            {
                return BadRequest("UserJob id is null");
            }
        }
        [HttpDelete("DeleteUserJob/{id}")]
        public async Task<IActionResult> Deleteuser(long Id)
        {
            var userjob = await _context.UserJob.FindAsync(Id);
            if (Id != null)
            {
                userjob.Status = Status.Blocked.ToString();
                _context.UserJob.Update(userjob);
                _context.SaveChanges();
                return Ok(userjob);

            }
            else
            {
                return BadRequest("UserJob id is null");

            }
        }
        [HttpGet("GetAlluser")]
        public async Task<IActionResult> GetAlluser()
        {
            var allusers = _context.UserJob.Where(x => x.Status == Status.Active.ToString()).ToList();
            return Ok(allusers);
        }
        [HttpGet("GetApplicantsByJob/{jobId}")]
        public async Task<IActionResult> GetApplicantsByJob(long jobId)
        {
            Console.WriteLine($"Checking JobId: {jobId}");

            var userJobEntries = await _context.UserJob
                .Where(uj => uj.JobId == jobId && uj.Status == "Active")
                .Select(uj =>uj.UserId).ToListAsync();
            var userList = new List<UserDTO>();
            foreach(var u in  userJobEntries)
            {
                var getuser = _userRequestProducer. RequestUserById(u);
                Console.WriteLine(getuser);
            }

            Console.WriteLine($"Total UserJob entries found: {userJobEntries.Count}");
            
           
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("No user found in cache.");
            }

            // Check join condition
            //    var applicants = await (from uj in _context.UserJob
            //                            join u in _context.Users on uj.UserId.Trim() equals u.Id.Trim()
            //                            join j in _context.JobPosts on uj.JobId equals j.Id
            //                            where uj.JobId == jobId && uj.Status == "Active"
            //                            select new
            //                            {
            //                                //UserId = u.Id,
            //                                //UserName = u.Name,
            //                                JobId = j.Id,
            //                                JobTitle = j.Name,
            //                                JobDescription = j.Description,
            //                                JobLocation = j.Location,
            //                                JobSalary = j.MinSalary,
            //                                PostedDate = j.DatePosted
            //                            }).ToListAsync();
            //    foreach (var uj in userJobEntries)
            //    {
            //        Console.WriteLine($"Checking Join Condition - UserJob UserId: '{uj.UserId}'");
            //    }

            //    foreach (var u in users)
            //    {
            //        Console.WriteLine($"Checking Join Condition - Users Id: '{u.Id}'");
            //    }


            //    Console.WriteLine($"Total applicants found: {applicants.Count}");

            //    if (applicants.Count == 0)
            //    {
            //        Console.WriteLine("No applicants found for this job.");
            //        return NotFound(new { message = "No applicants found for this job." });
            //    }
            //    return Ok(applicants);
            //}
            return Ok("hello");
        }
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetUser(Guid id)
        //{
        //    if (!memoryCache.TryGetValue($"User{id}", out PublishedUser user) || user==null)
        //    {
        //        // Cache miss, now call Users Service API
        //        using (var httpClient = new HttpClient())
        //        {
        //            var response = await httpClient.GetAsync($"http://localhost:5286/api/Users/GetUser/{id}");

        //            if (!response.IsSuccessStatusCode)
        //            {
        //                return NotFound("User not found in DB.");
        //            }

        //            var userData = await response.Content.ReadAsStringAsync(); // Get raw JSON

        //            // Cache the JSON string instead of an object
        //            memoryCache.Set($"User{id}", userData, TimeSpan.FromMinutes(30));

        //            return Content(userData, "application/json"); // Return as raw JSON
        //        }
        //    }

        //    return Content("ok"); // Return cached JSON
        //}
    }

}
