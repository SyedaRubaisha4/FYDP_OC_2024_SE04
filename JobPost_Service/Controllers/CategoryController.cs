using JobPost_Service.Data;
using JobPost_Service.Models;
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
            return await _context.Categories.ToListAsync();
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

        // POST: api/Category
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("Addcategory")]
        public async Task<ActionResult<Category>> PostCategory(CategoryDTO CategoryDto)
        {
          
            //if (!ModelState.IsValid)
            //{
            //    _logger.LogWarning("Invalid model state for the reequest.");
            //    return BadRequest(ModelState);
            //}
            Console.WriteLine("Received Payload:");
            Console.WriteLine(JsonConvert.SerializeObject(CategoryDto));

            // Map DTO to Entity
            var Category = new Category
            {
                Name = CategoryDto.Name,
                CategoryJobs = CategoryDto.CategoryJobs,

            };
            if (CategoryDto.CategoryImage != null && CategoryDto.CategoryImage.Length > 0)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Image");
                Directory.CreateDirectory(uploadFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(CategoryDto.CategoryImage.FileName);
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await CategoryDto.CategoryImage.CopyToAsync(fileStream);
                }

                Category.CategoryImageName = $"{uniqueFileName}";

            }

            // Add to database and save changes
            _context.Categories.Add(Category);
            await _context.SaveChangesAsync();

           
            // Return the created entity
            return CreatedAtAction("GetCategory", new { id = Category.Id }, Category);
        }
        // DELETE: api/Category/5
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

    }
}
