using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPost_Service.Data;
using JobPost_Service.Models;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;

namespace JobPost_Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicePostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;  // Inject IMemoryCache

        public ServicePostController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;  
            _memoryCache = memoryCache;
        }

        // GET: api/ServicePost
        [HttpGet]
        public async Task<IActionResult> GetServicePosts()
        {
            var servicePosts = await _context.ServicePosts.ToListAsync();
            return Ok(servicePosts);
        }

        // GET: api/ServicePost/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServicePost(int id)
        {
            var servicePost = await _context.ServicePosts.FindAsync(id);

            if (servicePost == null)
            {
                return NotFound();
            }

            return Ok(servicePost);
        }

        // POST: api/ServicePost
        [HttpPost]
        public async Task<ActionResult<ServicePost>> CreateServicePost(ServicePost servicePost)
        {
            // Automatically set values for missing fields
            if (!_memoryCache.TryGetValue("User", out PublishedUser user))
            {
                return BadRequest("No user found in cache.");
            }

            servicePost.UserId = user?.Id ?? "123"; // Replace with user logic
        //    servicePost.CategoryId = 1; // Replace with a default or determined category ID
            servicePost.Status = PostStatus.Pending; // Default status
            servicePost.DatePosted = DateTime.UtcNow; // Current date and time in UTC

            // Add the ServicePost to the context
            _context.ServicePosts.Add(servicePost);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetServicePost", new { id = servicePost.Id }, servicePost);
        }

        // PUT: api/ServicePost/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServicePost(int id, ServicePost servicePost)
        {
            if (id != servicePost.Id)
            {
                return BadRequest();
            }

            _context.Entry(servicePost).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServicePostExists(id))
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

        // DELETE: api/ServicePost/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicePost(int id)
        {
            var servicePost = await _context.ServicePosts.FindAsync(id);
            if (servicePost == null)
            {
                return NotFound();
            }

            _context.ServicePosts.Remove(servicePost);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServicePostExists(int id)
        {
            return _context.ServicePosts.Any(e => e.Id == id);
        }
    }
}
