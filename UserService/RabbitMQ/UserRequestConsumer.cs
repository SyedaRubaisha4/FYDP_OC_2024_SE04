using MassTransit;
using Microsoft.Extensions.Caching.Memory;
using SharedLibrary;
using UserService.Data;
using UserService.Models;

public class UserRequestConsumer : IConsumer<UserRequestMessage>
{
    private readonly IMemoryCache _cache;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public UserRequestConsumer(IMemoryCache cache, IServiceScopeFactory serviceScopeFactory)
    {
        _cache = cache;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Consume(ConsumeContext<UserRequestMessage> context)
    {
        var userId = context.Message.UserId;
        Console.WriteLine($"[UserRequestConsumer] Received request for user ID: {userId}");

        // 🔹 Try to get the user from cache
        if (_cache.TryGetValue(userId, out ApplicationUser cachedUser))
        {
            Console.WriteLine($"[UserRequestConsumer] User {userId} found in cache!");
            await context.RespondAsync(new UserResponseMessage { User = cachedUser });
            return;
        }

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user =  dbContext.Users
                .Where(x => x.Status == "Active" && x.Id == userId).Select(x => x);
              // .FirstOrDefaultAsync();

            if (user == null)
            {
                Console.WriteLine($"[UserRequestConsumer] No user found with ID {userId}");
                await context.RespondAsync(new UserResponseMessage { User = null });
                return;
            }

          //  Console.WriteLine($"[UserRequestConsumer] Found user {user.Id}, storing in cache...");

            // ✅ Store in cache for future requests
            _cache.Set(userId, user, TimeSpan.FromMinutes(30));

            await context.RespondAsync(new UserResponseMessage { User = user });
        }
    }
}
