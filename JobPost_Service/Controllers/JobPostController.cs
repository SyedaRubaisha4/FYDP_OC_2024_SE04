using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPost_Service.Data;
using JobPost_Service.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using SharedLibrary;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using JobPost_Service.Helper;
using MassTransit.Transports;
using JobPost_Service.RabbitMQ;
using MassTransit;
using JobPost_Service.Models.DTOs;
namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly UserRequestProducer _userRequestProducer;
        private readonly IPublishEndpoint _publishEndpoint;

        public JobPostController(IPublishEndpoint publishEndpoint,ApplicationDbContext context, IMemoryCache memoryCache, UserRequestProducer UserRequestProducer)
        {
            _context = context;
            _memoryCache = memoryCache;
            _userRequestProducer = UserRequestProducer;
            _publishEndpoint = publishEndpoint;
        }
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetJobPostsByUser(string userId)
        {
            var jobPosts = await _context.JobPosts
                .Where(j => j.UserId == userId)
                .ToListAsync();

            if (jobPosts == null || jobPosts.Count == 0)
            {
                return NotFound("No job posts found for this user.");
            }

            // Fetch only user image from User Service
            PublishedUser user = await _userRequestProducer.RequestUserById(userId);

            var result = new
            {
                UserImage = user.UserImage, // Only return profile image
                Jobs = jobPosts
            };

            return Ok(result);
        }

        // GET: api/JobPost
        [HttpGet]
        public async Task<IActionResult> GetJobPosts()
        {
            var jobPosts = await _context.JobPosts.ToListAsync();

            if (jobPosts == null || jobPosts.Count == 0)
            {
                return NotFound("No job posts found.");
            }

            var result = new List<object>();

            foreach (var job in jobPosts)
            {
                var user = await _userRequestProducer.RequestUserById(job.UserId); // Fetch user one by one

                result.Add(new
                {
                    job.Id,
                    job.Name,
                    job.Description,
                    job.UserId,
                    job.MinSalary,
                    job.MaxSalary,
                    job.WorkplaceType,
                    job.CompanyName,
                    job.Location,
                    job.JobType,
                    job.Address,
                    job.DatePosted,
                    UserImage = user?.UserImage // Attach user image
                });
            }

            return Ok(result);
        }


        // GET: api/JobPost/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobPost(int id)
        {
            var jobPost = await _context.JobPosts.FindAsync(id);

            if (jobPost == null)
            {
                return NotFound();
            }

            return Ok(jobPost);
        }

        
        [HttpPost]
        
        public async Task<ActionResult<JobPost>> CreateJobPost(JobPost jobPost)
        {
            //if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            //{
            //    return BadRequest("No user found in cache.");
            //}

            // User ID & default values assign karna
            jobPost.UserId = jobPost.UserId;
            jobPost.Status = Status.Active.ToString();
            jobPost.Type = "Job";
            jobPost.DatePosted = DateTime.UtcNow;

            // JobPost save karna
            _context.JobPosts.Add(jobPost);

            // 🛠️ Category count update karna
            var category = await _context.Categories.FindAsync(jobPost.CategoryId);
            if (category != null)
            {
                category.CategoryCount += 1; // Count increment
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobPost", new { id = jobPost.Id }, jobPost);
        }




        // PUT: api/JobPost/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobPost(int id, JobPostUpdateDto jobPost)
        {
            var job=_context.JobPosts.Find(id);
            if ( job==null)
            {
                return BadRequest("no job exist");
            }

           job.Name=jobPost.Name;
            job.Description=jobPost.Description;
            job.Location= jobPost.Location;
            job.Address= jobPost.Address;
            job.PhoneNumber= jobPost.PhoneNumber;
            job.Email=jobPost.Email;
            job.MaxSalary=
                jobPost.MaxSalary;
            job.MinSalary= jobPost.MinSalary;
            job.JobType=jobPost.JobType;
            job.WorkplaceType=jobPost.WorkplaceType;
            job.Skills=jobPost.Skills;
            job.CompanyName=jobPost.CompanyName;
            _context.JobPosts.Update(job);
            _context.SaveChanges();
            return Ok(job);
        }

        // DELETE: api/JobPost/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJobPost(int id)
        {
            var jobPost = await _context.JobPosts.FindAsync(id);
            if (jobPost == null)
            {
                return NotFound();
            }
            jobPost.Status=Status.Blocked.ToString();
            _context.JobPosts.Update(jobPost);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool JobPostExists(int id)
        {
            return _context.JobPosts.Any(e => e.Id == id);
        }

        [HttpGet("GetAllJobCount")]
        public async Task<IActionResult> GetAllJobCount()
        {
            var jobs = await _context.JobPosts.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(jobs);
        }

        [HttpGet("GetJobWeeklyCount")]
        public async Task<IActionResult> GetJobWeeklyCount()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisWeek = await _context.JobPosts
               .Where(x => x.Status == Status.Active.ToString() && x.DatePosted >= startOfWeek)
               .CountAsync();
            return Ok(usersThisWeek);
        }
        [HttpGet("GetJobMonthlyCount")]
        public async Task<IActionResult> GetJobMonthlyCount()
        {
            var now = DateTime.UtcNow; var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisMonth = await _context.JobPosts
             .Where(x => x.Status == Status.Active.ToString() && x.DatePosted >= startOfMonth)
             .CountAsync();
            return Ok(usersThisMonth);
        }
    }
}
