//using JobPost_Service.Models;
using Microsoft.Extensions.Caching.Memory;  // Required for IMemoryCache
using Microsoft.Extensions.Logging;
using MassTransit;
using SharedLibrary;

public class UserConsumer : IConsumer<PublishedUser>
{
    private readonly IMemoryCache _memoryCache;  // Inject IMemoryCache
    private readonly ILogger<UserConsumer> _logger;

    public UserConsumer(IMemoryCache memoryCache, ILogger<UserConsumer> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
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
            _memoryCache.Set("User", user);  // Store the user object in memory cache

            _logger.LogInformation("Successfully processed and stored user: {Name}, {PhoneNumber}, {ID}", user.Name, user.PhoneNumber, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user: {Name}, {PhoneNumber}", user?.Name, user?.PhoneNumber);
        }
    }
}
