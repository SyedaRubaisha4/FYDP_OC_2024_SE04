using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SharedLibrary;
using System.Threading.Tasks;

public class NotificationUserConsumer : IConsumer<PublishedUser>
{
    private readonly ILogger<NotificationUserConsumer> _logger;
    private readonly IMemoryCache _memoryCache;


    public NotificationUserConsumer(ILogger<NotificationUserConsumer> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public async Task Consume(ConsumeContext<PublishedUser> context)
    {
        var user = context.Message;

        if (user == null)
        {
            _logger.LogWarning("Received a null user object in Notification Service.");
            return;
        }

        try
        {
            _memoryCache.Set("User", user);
            _logger.LogInformation("Notification Service received user: {Name}, {PhoneNumber}", user.Name, user.PhoneNumber);
            // Yahan notification logic likhein (e.g., send email, push notification, etc.)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user in Notification Service: {Name}, {PhoneNumber}", user.Name, user.PhoneNumber);
        }
    }
}
