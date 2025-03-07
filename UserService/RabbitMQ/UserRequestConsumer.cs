using MassTransit;
using Microsoft.EntityFrameworkCore;
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

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("[UserRequestConsumer] Fetching user from DB...");
            var user = await dbContext.Users
                .Where(x => x.Status == "Active" && x.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                Console.WriteLine($"[UserRequestConsumer] No user found with ID {userId}");
                await context.RespondAsync(new UserResponseMessage { User = null });
                return ;
            }

            Console.WriteLine($"[UserRequestConsumer] Found user: {user}");
            var PublishedUser = new PublishedUser
            {
                City=user.City,
                Experience=user.Experience, 
                Name=user.Name,
                PhoneNumber=user.PhoneNumber,
                Job=user.Job,
                UserImage=user.UserImageName,
                Id=user.Id
            };

            await context.RespondAsync(new UserResponseMessage { User = PublishedUser });
        }
    }


}
