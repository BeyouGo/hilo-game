using HiLoGame.Api;
using HiLoGame.Application.Features.Authentication;
using HiLoGame.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HiLoGame.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HiLoGame.Domain.Aggregates.Player;
using HiLoGame.Api.Middlewares;
using HiLoGame.Infrastructure.Realtime;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;
// Add services to the container.


builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        );
    }); 

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<HiLoGameDbContext>(options =>
{

    var cs = builder.Configuration.GetConnectionString("DataContext");
    if (string.IsNullOrWhiteSpace(cs))
    {
        var host = builder.Configuration["DB_HOST"] ?? "db";
        var port = builder.Configuration["DB_PORT"] ?? "1433";
        var name = builder.Configuration["DB_NAME"] ?? "hilo-db";
        var user = builder.Configuration["DB_USER"] ?? "sa";
        var pass = builder.Configuration["DB_PASSWORD"] ?? "Your_strong_password123!";

        var b = new SqlConnectionStringBuilder
        {
            DataSource = $"{host},{port}",
            InitialCatalog = name,
            UserID = user,
            Password = pass,
            TrustServerCertificate = true,
            Encrypt = false
        };
        cs = b.ToString();
    }

    options.UseSqlServer(cs);
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext"));
});

builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<RegisterPlayer.Command>());

builder.Services
    .AddIdentityCore<Player>(opt =>
    {
        opt.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<HiLoGameDbContext>()
    .AddDefaultTokenProviders();



builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    );
});



var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JwtSettings:Secret"]!));
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            ValidIssuer = cfg["JwtSettings:Issuer"],
            ValidAudience = cfg["JwtSettings:Audience"],
            IssuerSigningKey = key
        };
        options.Events = new JwtBearerEvents
        {
            // Allow token via query string for WebSockets to /hubs/game
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/game"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDevFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200", 
                "http://localhost:8080", // dev docker
                "http://127.0.0.1:4200") // dev server/hub
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
ServicesConfigurations.ConfigureServices(builder.Services);

builder.Services.AddProblemDetails();              // ASP.NET Core built-in ProblemDetailsFactory
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("EF:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HiLoGameDbContext>(); // <-- your DbContext
    try
    {
        await db.Database.MigrateAsync(); // idempotent: runs only pending migrations
        // Optionally: seed
        // var seeder = scope.ServiceProvider.GetService<IDatabaseSeeder>();
        // if (seeder is not null) await seeder.SeedAsync(db, CancellationToken.None);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Failed to apply EF Core migrations on startup.");
        // For dev you might rethrow to fail fast: throw;
    }
}


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors("AllowDevFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.MapHub<HiLoGameHub>("/hubs/game");

app.Run();
