using JobPost_Service.Data;
using JobPost_Service.Helper;
using JobPost_Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace JobService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }
        // GET: api/Category
        [HttpGet("AllCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategory()
        {
            var categories =await _context.Categories.Where(x => x.Status == Status.Active.ToString()).ToListAsync();
            return categories;
        }

        // GET: api/Category/5
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

        // PUT: api/Category/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

            // Add to database and save changes
            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();

           
            // Return the created entity
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

            _context.Categories.Remove(Category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
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
                        j.CategoryId
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
                        s.CategoryId
                    })
                    .ToListAsync();

                if (!jobs.Any() && !services.Any())
                {
                    return NotFound(new { Message = "No jobs or services found for this category." });
                }

                var response = new Dictionary<string, object>();

                if (jobs.Any()) response["Jobs"] = jobs;
                if (services.Any()) response["Services"] = services;

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Error = "An error occurred.", Details = ex.Message });
            }
        }
        [HttpGet("GetAllJobsAndServices")]
        public async Task<IActionResult> GetAllJobsAndServices()
        {
            try
            {
                var jobs = await _context.JobPosts
                    .AsNoTracking()
                    .ToListAsync();

                var services = await _context.ServicePosts
                    .AsNoTracking()
                    .ToListAsync();

                if (!jobs.Any() && !services.Any())
                {
                    return NotFound(new { Message = "No jobs or services found." });
                }

                return Ok(new { Jobs = jobs, Services = services });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching jobs and services: {ex.Message}");
                return StatusCode(500, new { Error = "An error occurred.", Details = ex.Message });
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

        [HttpGet("CategoryCount")]
        public async Task<IActionResult> CategoryCount(string Name)
        {
            var category= await _context.Categories.Where(x => x.Name == Name).FirstOrDefaultAsync();
            category.CategoryCount = category.CategoryCount + 1;
            _context.Categories.Update(category);
            return Ok(category);
        }
    }
}
