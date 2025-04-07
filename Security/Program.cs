
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Security.Data;
using SharedLibrary;

namespace Security
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
            builder.Services.AddMassTransit(x =>
            {
                x.AddRequestClient<GetUserByIdRequest>(); // ?? Register the client
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq://98.70.57.195", h =>
                    {
                        h.Username("user");  // Assuming you're still using the default username
                        h.Password("123456");  // Assuming you're still using the default password
                    });


                });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
