using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPost_Service.Data;
using JobPost_Service.Models;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;
using Microsoft.AspNetCore.Authorization;
using JobPost_Service.Helper;

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
             if (!_memoryCache.TryGetValue("User", out PublishedUser user))
                {
                    return BadRequest("No user found in cache.");
                }

            servicePost.UserId = user .Id?? "123"; 
            servicePost.Status = Status.Active.ToString(); 
           
            servicePost.DatePosted = DateTime.UtcNow; 

          
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
      
        [HttpGet("GetAllServiceCount")]
        public async Task<IActionResult> GetAllServiceCount()
        {
            var jobs = await _context.ServicePosts.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(jobs);
        }

        
        [HttpGet("GetServiceWeeklyCount")]
        public async Task<IActionResult> GetServiceWeeklyCount()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisWeek = await _context.ServicePosts
               .Where(x => x.Status == Status.Active.ToString() && x.DatePosted >= startOfWeek)
               .CountAsync();
            return Ok(usersThisWeek);
        }

        [HttpGet("GetServiceMonthlyCount")]
        public async Task<IActionResult> GetServiceMonthlyCount()
        {
            var now = DateTime.UtcNow; var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisMonth = await _context.ServicePosts
             .Where(x => x.Status == Status.Active.ToString() && x.DatePosted >= startOfMonth)
             .CountAsync();
            return Ok(usersThisMonth);
        }
        private bool ServicePostExists(int id)
        {
            return _context.ServicePosts.Any(e => e.Id == id);
        }    


    }
}
