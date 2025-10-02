
using System.Text;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FinanceApp.Api.Infrastructure.Data;
using FinanceApp.Api.Application.Auth;
using FinanceApp.Api.Application.Jobs;
using FinanceApp.Api.Application.Email;
using FinanceApp.Api.Application.Notifications;

namespace FinanceApp.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuration
        var configuration = builder.Configuration;

        // Get database connection string with fallback
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        var connectionString = databaseUrl 
                             ?? configuration.GetConnectionString("DefaultConnection")
                             ?? "Host=localhost;Port=5432;Database=finance_app;Username=postgres;Password=postgres";
        
        Console.WriteLine($"DATABASE_URL environment variable: {(string.IsNullOrEmpty(databaseUrl) ? "NOT SET" : "SET")}");
        Console.WriteLine($"Using connection string: {(string.IsNullOrEmpty(connectionString) ? "EMPTY" : "CONFIGURED")}");

        // EF Core DbContext
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // CORS
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        // Authentication - JWT
        var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") 
                   ?? configuration["Jwt:Key"] 
                   ?? "default-development-key-not-for-production";
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        builder.Services.AddAuthorization();

        // Token service
        builder.Services.AddSingleton<ITokenService, TokenService>();

        // Email service
        builder.Services.AddScoped<IEmailService, SmtpEmailService>();

        // Notification service
        builder.Services.AddScoped<INotificationService, NotificationService>();

        // Hangfire - only enable if we have a valid connection string
        if (!string.IsNullOrEmpty(connectionString) && connectionString != "Host=localhost;Port=5432;Database=finance_app;Username=postgres;Password=postgres")
        {
            try
            {
                builder.Services.AddHangfire(config =>
                {
                    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                          .UseSimpleAssemblyNameTypeSerializer()
                          .UseRecommendedSerializerSettings()
                          .UsePostgreSqlStorage(connectionString);
                });
                builder.Services.AddHangfireServer(options =>
                {
                    options.Queues = new[] { configuration["Hangfire:Queue"] ?? "default" };
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Hangfire: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Skipping Hangfire initialization - no valid database connection string");
        }

        // Controllers & Minimal APIs support
        builder.Services.AddControllers();

        // Jobs
        builder.Services.AddScoped<ISalaryCreditJob, SalaryCreditJob>();
        builder.Services.AddScoped<INeedsDeductionJob, NeedsDeductionJob>();

        // Swagger with JWT support
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FinanceApp API", Version = "v1" });
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token"
            };
            c.AddSecurityDefinition("Bearer", securityScheme);
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }, new string[]{}
                }
            });
        });

        var app = builder.Build();

        // Middleware pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("DefaultCors");

        app.UseAuthentication();
        app.UseAuthorization();

        // Map controllers
        app.MapControllers();

        // Example endpoint remains for health check
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet("/weatherforecast", () =>
        {
            var forecast =  Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = summaries[Random.Shared.Next(summaries.Length)]
                })
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast")
        .WithOpenApi();

        // Apply database migrations in production
        if (app.Environment.IsProduction())
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    // Don't throw - let the app start even if migration fails
                }
            }
        }

        // Hangfire dashboard (dev only)
        if (app.Environment.IsDevelopment())
        {
            app.UseHangfireDashboard("/hangfire");
        }

        // Schedule recurring jobs after app starts
        try
        {
            using (var scope = app.Services.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                
                // Schedule recurring job daily at 00:05 UTC
                recurringJobManager.AddOrUpdate<ISalaryCreditJob>(
                    "monthly-salary-credit",
                    job => job.RunAsync(CancellationToken.None),
                    "5 0 * * *");

                recurringJobManager.AddOrUpdate<INeedsDeductionJob>(
                    "daily-needs-deduction",
                    job => job.RunAsync(CancellationToken.None),
                    "10 0 * * *");
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail startup
            Console.WriteLine($"Failed to schedule recurring jobs: {ex.Message}");
        }

        app.Run();
    }
}

