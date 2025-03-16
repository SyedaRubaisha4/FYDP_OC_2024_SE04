using JobPost_Service.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MassTransit;
using SharedLibrary;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using JobPost_Service.Data;

public class UserConsumer : IConsumer<PublishedUser>
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<UserConsumer> _logger;
    private readonly ApplicationDbContext _context;  // Inject database context

    public UserConsumer(IMemoryCache memoryCache, ILogger<UserConsumer> logger, ApplicationDbContext context)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _context = context;  // Initialize database context
    }

    public async Task Consume(ConsumeContext<PublishedUser> context)
    {
        var user = context.Message;

        if (user == null)
        {
            _logger.LogWarning("Received a null user object.");
            return;
        }

        try
        {
            // Store user in the in-memory cache
            _memoryCache.Set("User", user);

            _logger.LogInformation("Stored user in cache: {Name}, {PhoneNumber}, {ID}", user.Name, user.PhoneNumber, user.Id);

            // Increase category count directly here
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Name == user.Job);
            if (category != null)
            {
                category.CategoryCount += 1;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();  // Save changes to the database

                _logger.LogInformation("Category count updated for category: {CategoryName}, New Count: {Count}", category.Name, category.CategoryCount);
            }
            else
            {
                _logger.LogWarning("Category not found for job: {Job}", user.Job);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user: {Name}, {PhoneNumber}", user?.Name, user?.PhoneNumber);
        }
    }
}
