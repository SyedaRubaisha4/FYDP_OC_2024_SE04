
using Location_Service.Data;
using Microsoft.EntityFrameworkCore;
using SharedLibrary;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using MassTransit;
using Location_Service.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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


builder.Services.AddScoped<IConsumer<PublishedUser>, UserConsumer>();

builder.Services.AddScoped<UserRequestProducer>();



// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Register UserConsumer for dependency injection
//builder.Services.AddScoped<IConsumer<PublishedUser>, UserConsumer>();

//builder.Services.AddScoped<UserRequestProducer>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();


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
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
