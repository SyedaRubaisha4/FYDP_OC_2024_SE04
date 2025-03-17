
using FeedbackService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;

namespace FeedbackService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register FeedbackGateway as a service
            //
            //builder.Services.AddScoped<FeedbackGateway>();

            // Add MassTransit and RabbitMQ
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<UserConsumer>();  // Register the consumer

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq://localhost", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    // Set up the consumer with the exchange binding
                    cfg.ReceiveEndpoint("user-feedback-consumer-queue", e =>
                    {
                        e.ConfigureConsumer<UserConsumer>(context);

                        // Bind to the "user-exchange" using message topology
                        e.Bind("user-exchange", binding =>
                        {
                            binding.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout;  // Use Fanout exchange type
                        });
                    });
                });
            });

            // Register UserConsumer for dependency injection
            builder.Services.AddScoped<IConsumer<PublishedUser>, UserConsumer>();

            // User Service for the storage of data
            // Add Memory Cache to the container
            builder.Services.AddMemoryCache();
            builder.Services.AddDataProtection();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
