using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using SharedLibrary;

namespace NotificationService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddMassTransit(x =>
            {
                // Register consumers
                x.AddConsumer<NotificationUserConsumer>();
                x.AddConsumer<AcceptedJobNotificationConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq://98.70.57.195", h =>
                    {
                        h.Username("user");  // Assuming you're still using the default username
                        h.Password("123456");  // Assuming you're still using the default password
                    });


                    cfg.ReceiveEndpoint("notification_service_queue", e =>
                    {
                        e.ConfigureConsumer<NotificationUserConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("accepted-job-queue", e =>
                    {
                        e.ConfigureConsumer<AcceptedJobNotificationConsumer>(context);
                    });
                });
            });

            builder.Services.AddMemoryCache();
            builder.Services.AddDataProtection();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting(); 

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            app.Run();
        }
    }
}
