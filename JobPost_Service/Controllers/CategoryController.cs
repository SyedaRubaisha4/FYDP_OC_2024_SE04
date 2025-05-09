﻿using JobPost_Service.Data;
using JobPost_Service.Helper;
using JobPost_Service.Models;
using JobPost_Service.RabbitMQ;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using SharedLibrary;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly UserRequestProducer _userRequestProducer;
        private readonly IPublishEndpoint _publishEndpoint;

        private readonly ILogger<CategoryController> _logger;
        public CategoryController(IPublishEndpoint publishEndpoint,ApplicationDbContext context, ILogger<CategoryController> logger, IMemoryCache memoryCache, UserRequestProducer UserRequestProducer)
        {
            _context = context;
            _logger = logger;
            _memoryCache= memoryCache;
            _userRequestProducer = UserRequestProducer;
            _publishEndpoint = publishEndpoint;
        }
        [HttpGet("AllCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            var categories =await _context.Categories.Where(x => x.Status == Status.Active.ToString()).ToListAsync();
            return categories;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var Category = await _context.Categories.FindAsync(id);

            if (Category == null)
            {
                return NotFound();
            }

            return Category;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category Category)
        {
            if (id != Category.Id)
            {
                return BadRequest();
            }

            _context.Entry(Category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
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

        [HttpPost("Addcategory")]
        public async Task<ActionResult<Category>> PostCategory(CategoryDTO CategoryDto)
        {
   
            var Category = new Category
            {
                Name = CategoryDto.Name,
                Status=Status.Active.ToString(),
                CreatedDate=DateTime.Now,
                CategoryCount = 0,

            };
            if (CategoryDto.CategoryImage != null && CategoryDto.CategoryImage.Length > 0)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "CategoryImages");

                Directory.CreateDirectory(uploadFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CategoryDto.CategoryImage.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CategoryDto.CategoryImage.CopyToAsync(fileStream);
                }

                Category.CategoryImageName = $"{uniqueFileName}";

            }
            else
            {
                Category.CategoryImageName = null;
            }

            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCategory", new { id = Category.Id }, Category);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var Category = await _context.Categories.FindAsync(id);
            if (Category == null)
            {
                return NotFound();
            }
            Category.Status=Status.Blocked.ToString();

            _context.Categories.Update(Category);

            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
     //   [HttpGet("{id}/posts")]
        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetJobsAndServicesByCategory(int id)
        {
            try
            {
                var jobs = await _context.JobPosts
                    .AsNoTracking()
                    .Where(j => j.CategoryId == id)
                    .Select(j => new
                    {
                        j.Id,
                        j.Name,
                        j.WorkplaceType,
                        j.Timing,
                        j.Address,
                        j.CompanyName,
                        j.Skills,
                        j.JobType,
                        j.Description,
                        j.DatePosted,
                        j.Location,
                        j.Experience,
                        j.MinSalary,
                        j.MaxSalary,
                        j.CategoryId,
                        j.UserId // Add UserId for fetching profile image
                    })
                    .ToListAsync();

                var services = await _context.ServicePosts
                    .AsNoTracking()
                    .Where(s => s.CategoryId == id)
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Description,
                        s.DatePosted,
                        s.Location,
                        s.Address,
                        s.MinSalary,
                        s.MaxSalary,
                        s.CategoryId,
                        s.UserId // Add UserId for fetching profile image
                    })
                    .ToListAsync();

                if (!jobs.Any() && !services.Any())
                {
                    return NotFound(new { Message = "No jobs or services found for this category." });
                }

                // Get unique user IDs from jobs and services
                var userIds = jobs.Select(j => j.UserId)
                                  .Union(services.Select(s => s.UserId))
                                  .Distinct()
                                  .ToList();

                // Fetch user images from User Service
                var userProfiles = new Dictionary<string, string>(); // <UserId, UserImage>
                foreach (var userId in userIds)
                {
                    var user = await _userRequestProducer.RequestUserById(userId);
                    if (user != null)
                    {
                        userProfiles[userId] = user.UserImage;
                    }
                }

                // Add Profile Image to Jobs and Services
                var jobsWithImages = jobs.Select(j => new
                {
                    j.Id,
                    j.Name,
                    j.WorkplaceType,
                    j.Timing,
                    j.Address,
                    j.CompanyName,
                    j.Skills,
                    j.JobType,
                    j.Description,
                    j.DatePosted,
                    j.Location,
                    j.Experience,
                    j.MinSalary,
                    j.MaxSalary,
                    j.CategoryId,
                    j.UserId,
                    ProfileImage = userProfiles.ContainsKey(j.UserId) ? userProfiles[j.UserId] : null
                }).ToList();

                var servicesWithImages = services.Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DatePosted,
                    s.Location,
                    s.Address,
                    s.MinSalary,
                    s.MaxSalary,
                    s.CategoryId,
                    s.UserId,
                    ProfileImage = userProfiles.ContainsKey(s.UserId) ? userProfiles[s.UserId] : null
                }).ToList();

                var response = new Dictionary<string, object>();

                if (jobsWithImages.Any()) response["Jobs"] = jobsWithImages;
                if (servicesWithImages.Any()) response["Services"] = servicesWithImages;

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "An error occurred.", Details = ex.Message });
            }
        }

        [HttpGet("GetAllJobsAndServices")]
        public async Task<IActionResult> GetAllJobsAndServices([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { Message = "UserId is required." });
            }

            try
            {
                var jobs = await _context.JobPosts
                    .Where(j => j.UserId == userId) // ✅ Filter by UserId
                    .AsNoTracking()
                    .ToListAsync();

                var services = await _context.ServicePosts
                    .Where(s => s.UserId == userId) // ✅ Filter by UserId
                    .AsNoTracking()
                    .ToListAsync();

                if (!jobs.Any() && !services.Any())
                {
                    return NotFound(new { Message = "No jobs or services found for this user." });
                }

                return Ok(new { Jobs = jobs, Services = services });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred.", Error = ex.Message });
            }
        }

     
        [HttpGet("GetAllCategoryCount")]
        public async Task<IActionResult> GetAllCategoryCount()
        {
            var Categorys = await _context.Categories.Where(x => x.Status == Status.Active.ToString()).CountAsync();
            return Ok(Categorys);
        }       
        [HttpGet("GetCategoryWeeklyCount")]
        public async Task<IActionResult> GetCategoryWeeklyCount()
        {
            var now = DateTime.UtcNow;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisWeek = await _context.Categories
               .Where(x => x.Status == Status.Active.ToString() && x.CreatedDate >= startOfWeek)
               .CountAsync();
            return Ok(usersThisWeek);
        }
      
        [HttpGet("GetCategoryMonthlyCount")]
        public async Task<IActionResult> GetCategoryMonthlyCount()
        {
            var now = DateTime.UtcNow; var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var usersThisMonth = await _context.Categories
             .Where(x => x.Status == Status.Active.ToString() && x.CreatedDate >= startOfMonth)
             .CountAsync();
            return Ok(usersThisMonth);
        }
       
    }
}
