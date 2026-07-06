using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JokerNutrition.Tests.Helpers;

/// <summary>
/// Custom WebApplicationFactory that swaps the SQL Server DbContext for an
/// in-memory EF Core database so tests run without any external dependencies.
/// </summary>
public class TestWebAppFactory : WebApplicationFactory<Program>
{
    private static int _dbCounter;

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

            // Ensure the DB is created and seeded with minimal test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private static void SeedTestData(JokerNutritionContext db)
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
                CreatedAt = DateTime.UtcNow,
                // BCrypt of "Coach@Test123!" — pre-computed to avoid Identity dependency
                PasswordHash = "AQAAAAIAAYagAAAAEKVIRSiRGJlD3JBHVSvlBMORlV+5kYmADnf63eiNGx5rnBfx4XS38cUHFzpCnN6R2w=="
            };
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
                CreatedAt = DateTime.UtcNow,
                PasswordHash = "AQAAAAIAAYagAAAAEKVIRSiRGJlD3JBHVSvlBMORlV+5kYmADnf63eiNGx5rnBfx4XS38cUHFzpCnN6R2w=="
            };
            db.Users.Add(athlete);
            db.UserRoles.Add(new UserRole { UserId = 2, RoleId = 3 });
            db.Athletes.Add(new Athlete { Id = 1, UserId = 2, AssignedCoachId = 1 });
        }

        db.SaveChanges();
    }
}
