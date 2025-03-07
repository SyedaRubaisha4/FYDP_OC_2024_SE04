using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.RabbitMQ;
using SharedLibrary;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserService.Models.JWT;
var builder = WebApplication.CreateBuilder(args);

SharedUtilityConnection.AppConfiguration = builder.Configuration;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));  // Set your connection string


// Add MassTransit and RabbitMQ
//builder.Services.AddMassTransit(x =>
//{
//    x.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host("rabbitmq://localhost", h =>
//        {
//            h.Username("guest");  // Default RabbitMQ username
//            h.Password("guest");  // Default RabbitMQ password
//        });

//        // Configure a Fanout Exchange
//        cfg.Message<PublishedUser>(configTopology =>
//        {
//            configTopology.SetEntityName("user-exchange"); // Set the exchange name
//        });

//        cfg.Publish<PublishedUser>(publishConfig =>
//        {
//            publishConfig.ExchangeType = RabbitMQ.Client.ExchangeType.Fanout; ; // Use "fanout" for broadcasting to multiple queues
//        });
//    });
//});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRequestConsumer>(); // ✅ Register the consumer

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq://localhost", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("user-service-queue", e =>
        {
            e.ConfigureConsumer<UserRequestConsumer>(context);
        });
    });
});


builder.Services.AddScoped<IUserProducer, UserProducer>();
builder.Services.AddScoped<JwtTokenHelper>();



// Register UserProducer for publishing messages
//builder.Services.AddScoped<IUserProducer, UserProducer>();

// Add Session service
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;               // Make session cookie HTTP-only
    options.Cookie.IsEssential = true;            // Essential for GDPR compliance
});

// Add HttpContextAccessor for session access
builder.Services.AddHttpContextAccessor();


// Add controllers
builder.Services.AddControllers();

// Register HttpClient
builder.Services.AddHttpClient();

// Add Swagger for API documentation (Optional, but useful for testing APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("V1", new OpenApiInfo
    {
        Version = "V1",
        Title = "CompCorePro",
        Description = "CompCorePro WebAPI"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
});
});

// Add CORS policy (Allow all origins for development purposes)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var config = SharedUtilityConnection.AppConfiguration;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["JWT:ValidIssuer"],               // ? No more NullReferenceException
        ValidAudience = config["JWT:ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["JWT:Secret"])
        )
    };
});
builder.Services.AddMemoryCache();
builder.Services.AddScoped<UserRequestConsumer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
        options.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/V1/swagger.json", "FYP APIs");
    });
    app.UseCors("AddCors");
    //using (var scope = app.Services.CreateScope())
    //{
    //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //    db.Database.Migrate();
    //}
}

// Use HTTPS redirection
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseCors("AllowReactNativeApp");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
