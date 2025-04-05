using MassTransit;
using MassTransit.SqlTransport;
using Microsoft.Extensions.Caching.Memory;
using NotificationService.Data;
using NotificationService.Models;
using SharedLibrary;
using System;
using System.Threading.Tasks;

public class AcceptedJobNotificationConsumer : IConsumer<AcceptedJobNotificationEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    public AcceptedJobNotificationConsumer(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache= memoryCache;
    }

    public async Task Consume(ConsumeContext<AcceptedJobNotificationEvent> context)
    {
        var message = context.Message;

        var notification = new AcceptedJobNotifcation
        {
            SenderId = message.UserId,
            ReceiverId = message.ApplicantId,
            UserJobId = message.JobId,
            JobStatus = message.JobStatus,  
            CreatedDate = DateTime.UtcNow,
            Status="Active",
            IsSee=false,
            NotificationText=message.NotificationText,
        };
        //if (!_memoryCache.TryGetValue("User", out PublishedUser user))
        //{
        //  throw new Exception("No user found in cache.");
        //}


        _context.AcceptedJobNotifcation.Add(notification); 

        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ Notification saved: UserId={message.UserId}, JobId={message.JobId}");
    }
}
