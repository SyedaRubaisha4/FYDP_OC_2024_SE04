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

namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;  

        public JobPostController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
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
            return Ok(jobPosts);

            //var jobPosts = await _context.JobPosts.ToListAsync();
           

            //return Ok(jobPosts);
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
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("No user found in cache.");
            }


            jobPost.UserId = user?.Id;
            jobPost.Status = Status.Active.ToString();
            jobPost.DatePosted = DateTime.UtcNow;
             _context.JobPosts.Add(jobPost);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetJobPost", new { id = jobPost.Id }, jobPost);
        }



        // PUT: api/JobPost/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJobPost(int id, JobPost jobPost)
        {
            if (id != jobPost.Id)
            {
                return BadRequest();
            }

            _context.Entry(jobPost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobPostExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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

            _context.JobPosts.Remove(jobPost);
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
