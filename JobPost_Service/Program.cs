using JobPost_Service.Data;
using JobPost_Service.Models;
using JobPost_Service.RabbitMQ;

//using JobPost_Service.RabbitMQ;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Replace with your connection string

// Register IHttpContextAccessor before using it in UserConsumer
builder.Services.AddHttpContextAccessor();  // <-- Add this line

// Add MassTransit and RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserConsumer>();  // Register the consume
    x.AddRequestClient<UserRequestMessage>(new Uri("queue:user-service-queue"));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://98.70.57.195", h =>
        {
            h.Username("user");  // Assuming you're still using the default username
            h.Password("123456");  // Assuming you're still using the default password
        });

        // Set up the consumer with the exchange binding
        cfg.ReceiveEndpoint("user-consumer-queue", e =>
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

//builder.Services.AddScoped<IUserProducer, UserProducer>();

builder.Services.AddScoped<UserRequestProducer>();

// User Service for the storage of data
// Add Memory Cache to the container
builder.Services.AddMemoryCache();
builder.Services.AddDataProtection();

/*// Add session services
builder.Services.AddDistributedMemoryCache(); // In-memory cache for session data
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Prevent client-side access to the cookie
    options.Cookie.IsEssential = true; // Always include cookie for the session
});*/

// Add controllers
builder.Services.AddControllers();

// Add the logging
builder.Logging.AddConsole(); // Logs to the console
builder.Logging.AddDebug();

// CORS policy configuration (allowing React Native app to make requests)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactNativeApp", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactNativeApp");
app.UseHttpsRedirection();
app.UseAuthorization();

// Enable session middleware
//app.UseSession();

app.MapControllers();

app.Run();
