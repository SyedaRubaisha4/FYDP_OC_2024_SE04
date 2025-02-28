using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPost_Service.Data;
using JobPost_Service.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using SharedLibrary;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobPostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;  // Inject IMemoryCache

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

        
        // Ensure this endpoint requires authentication
        [HttpPost]
        public async Task<ActionResult<JobPost>> CreateJobPost(JobPost jobPost)
        {
            // Retrieve the user from in-memory cache
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("No user found in cache.");
            }


            jobPost.UserId = user?.Id;
            Console.WriteLine(jobPost.UserId);
            //  jobPost.CategoryId = 1; // Default category ID
            jobPost.Status = PostStatus.Pending; // Default status
            jobPost.DatePosted = DateTime.UtcNow;
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(jobPost));
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
    }
}
