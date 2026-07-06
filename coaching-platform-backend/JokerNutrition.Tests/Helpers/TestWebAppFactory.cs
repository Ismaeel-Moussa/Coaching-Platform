using Autofac;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JokerNutrition.Tests.Helpers;

/// <summary>
/// Custom WebApplicationFactory that swaps the SQL Server DbContext for an
/// in-memory EF Core database so tests run without any external dependencies.
/// </summary>
public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private static int _dbCounter;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Register fake email service in Autofac to override the real one
        builder.ConfigureContainer<ContainerBuilder>(cb =>
        {
            cb.RegisterType<FakeEmailService>().As<JokerNutrition.Business.Services.IEmailService>().InstancePerLifetimeScope();
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<JokerNutritionContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add an isolated in-memory DB per test factory instance
            var dbName = $"JokerNutritionTest_{Interlocked.Increment(ref _dbCounter)}";
            services.AddDbContext<JokerNutritionContext>(opts =>
                opts.UseInMemoryDatabase(dbName));

            // Bypass the .NET 10 PipeWriter.UnflushedBytes bug in TestServer
            services.PostConfigure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.OutputFormatters.Insert(0, new TestJsonOutputFormatter());
            });

            // Ensure the DB is created and seeded with minimal test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.IPasswordHasher<User>>();
            db.Database.EnsureCreated();
            SeedTestData(db, hasher);
        });
    }

    /// <summary>
    /// Custom text formatter that writes JSON directly to the Response Stream (bypassing PipeWriter).
    /// </summary>
    private class TestJsonOutputFormatter : Microsoft.AspNetCore.Mvc.Formatters.TextOutputFormatter
    {
        public TestJsonOutputFormatter()
        {
            SupportedMediaTypes.Add("application/json");
            SupportedMediaTypes.Add("text/json");
            SupportedMediaTypes.Add("application/*+json");
            SupportedEncodings.Add(System.Text.Encoding.UTF8);
        }

        public override async Task WriteResponseBodyAsync(
            Microsoft.AspNetCore.Mvc.Formatters.OutputFormatterWriteContext context, 
            System.Text.Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            if (context.Object != null)
            {
                var services = context.HttpContext.RequestServices;
                var mvcJsonOptions = services?.GetService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>()?.Value;
                
                var options = mvcJsonOptions?.JsonSerializerOptions ?? new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                };

                await System.Text.Json.JsonSerializer.SerializeAsync(
                    response.Body, 
                    context.Object, 
                    context.ObjectType ?? context.Object.GetType(),
                    options);
            }
        }
    }

    private class FakeEmailService : JokerNutrition.Business.Services.IEmailService
    {
        public Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role) => Task.CompletedTask;
        public Task SendPasswordResetEmailAsync(string toEmail, string resetUrl) => Task.CompletedTask;
    }

    private static void SeedTestData(JokerNutritionContext db, Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher)
    {
        // Roles
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new Role { Id = 2, Name = "Coach", NormalizedName = "COACH" },
                new Role { Id = 3, Name = "Athlete", NormalizedName = "ATHLETE" }
            );
        }

        // Test coach user
        if (!db.Users.Any(u => u.Email == "coach@test.com"))
        {
            var coach = new User
            {
                Id = 1,
                UserName = "coach@test.com",
                NormalizedUserName = "COACH@TEST.COM",
                Email = "coach@test.com",
                NormalizedEmail = "COACH@TEST.COM",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Coach",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            coach.PasswordHash = hasher.HashPassword(coach, "Coach@Test123!");
            db.Users.Add(coach);
            db.UserRoles.Add(new UserRole { UserId = 1, RoleId = 2 });
            db.Coaches.Add(new Coach { Id = 1, UserId = 1, IsActive = true });
        }

        // Test athlete user
        if (!db.Users.Any(u => u.Email == "athlete@test.com"))
        {
            var athlete = new User
            {
                Id = 2,
                UserName = "athlete@test.com",
                NormalizedUserName = "ATHLETE@TEST.COM",
                Email = "athlete@test.com",
                NormalizedEmail = "ATHLETE@TEST.COM",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Athlete",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            athlete.PasswordHash = hasher.HashPassword(athlete, "Athlete@Test123!");
            db.Users.Add(athlete);
            db.UserRoles.Add(new UserRole { UserId = 2, RoleId = 3 });
            db.Athletes.Add(new Athlete { Id = 1, UserId = 2, AssignedCoachId = 1 });
        }

        db.SaveChanges();
    }
}

