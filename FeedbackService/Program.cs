
using FeedbackService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using RabbitMQ.Client;
using FeedbackService.RabbitMQ;

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
                x.AddConsumer<UserConsumer>();
                x.AddRequestClient<UserRequestMessage>(new Uri("queue:user-service-queue"));

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq://98.70.57.195", h =>
                    {
                        h.Username("user");  // Assuming you're still using the default username
                        h.Password("123456");  // Assuming you're still using the default password
                    });

                    cfg.Publish<PublishedUser>(p =>
                    {
                        p.ExchangeType = ExchangeType.Fanout;  // ? Corrected Binding
                    });

                    cfg.ReceiveEndpoint("user-consumer-queue", e =>
                    {
                        e.ConfigureConsumer<UserConsumer>(context);
                    });
                });
            });


            // Register UserConsumer for dependency injection
            builder.Services.AddScoped<IConsumer<PublishedUser>, UserConsumer>();

            builder.Services.AddScoped<UserRequestProducer>();

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
