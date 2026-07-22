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
        builder.UseSetting("BlobStorageSettings:ConnectionString", "UseDevelopmentStorage=true");
        builder.UseSetting("BlobStorageSettings:ContainerName", "public-assets");
        builder.UseSetting("BlobStorageSettings:PrivateContainerName", "athlete-progress-photos");
        builder.UseSetting("BlobStorageSettings:LocalFallbackBaseUrl", "http://127.0.0.1:8765");

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
        public Task SendAccountStatusEmailAsync(string toEmail, string userName, bool isActive, string? reason = null) => Task.CompletedTask;
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

        // Admin intentionally has no Coach row, covering role-only admin access.
        if (!db.Users.Any(u => u.Email == "admin@test.com"))
        {
            var admin = new User
            {
                Id = 3,
                UserName = "admin@test.com",
                NormalizedUserName = "ADMIN@TEST.COM",
                Email = "admin@test.com",
                NormalizedEmail = "ADMIN@TEST.COM",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Admin",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@Test123!");
            db.Users.Add(admin);
            db.UserRoles.Add(new UserRole { UserId = 3, RoleId = 1 });
        }

        if (!db.Users.Any(u => u.Email == "reviewed-athlete@test.com"))
        {
            var reviewedAthlete = new User
            {
                Id = 4,
                UserName = "reviewed-athlete@test.com",
                NormalizedUserName = "REVIEWED-ATHLETE@TEST.COM",
                Email = "reviewed-athlete@test.com",
                NormalizedEmail = "REVIEWED-ATHLETE@TEST.COM",
                EmailConfirmed = true,
                FirstName = "Reviewed",
                LastName = "Athlete",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            reviewedAthlete.PasswordHash = hasher.HashPassword(reviewedAthlete, "Athlete@Test123!");
            db.Users.Add(reviewedAthlete);
            db.UserRoles.Add(new UserRole { UserId = 4, RoleId = 3 });
            db.Athletes.Add(new Athlete { Id = 2, UserId = 4, AssignedCoachId = 1 });
        }

        if (!db.Users.Any(u => u.Email == "other-coach@test.com"))
        {
            var otherCoach = new User
            {
                Id = 5,
                UserName = "other-coach@test.com",
                NormalizedUserName = "OTHER-COACH@TEST.COM",
                Email = "other-coach@test.com",
                NormalizedEmail = "OTHER-COACH@TEST.COM",
                EmailConfirmed = true,
                FirstName = "Other",
                LastName = "Coach",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            otherCoach.PasswordHash = hasher.HashPassword(otherCoach, "Coach@Test123!");
            db.Users.Add(otherCoach);
            db.UserRoles.Add(new UserRole { UserId = 5, RoleId = 2 });
            db.Coaches.Add(new Coach { Id = 2, UserId = 5, IsActive = true });
        }

        if (!db.Users.Any(u => u.Email == "other-athlete@test.com"))
        {
            var otherAthlete = new User
            {
                Id = 6,
                UserName = "other-athlete@test.com",
                NormalizedUserName = "OTHER-ATHLETE@TEST.COM",
                Email = "other-athlete@test.com",
                NormalizedEmail = "OTHER-ATHLETE@TEST.COM",
                EmailConfirmed = true,
                FirstName = "Other",
                LastName = "Athlete",
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };
            otherAthlete.PasswordHash = hasher.HashPassword(otherAthlete, "Athlete@Test123!");
            db.Users.Add(otherAthlete);
            db.UserRoles.Add(new UserRole { UserId = 6, RoleId = 3 });
            db.Athletes.Add(new Athlete { Id = 3, UserId = 6, AssignedCoachId = 2 });
        }

        if (!db.AthleteOnboardingAssessments.Any())
        {
            var submittedAt = DateTime.UtcNow.AddHours(-2);
            db.AthleteOnboardingAssessments.AddRange(
                new AthleteOnboardingAssessment
                {
                    Id = 1,
                    AthleteId = 1,
                    Status = JokerNutrition.Data.Enums.OnboardingAssessmentStatus.Submitted,
                    SubmittedAt = submittedAt,
                    CreatedAt = submittedAt,
                    UpdatedAt = submittedAt
                },
                new AthleteOnboardingAssessment
                {
                    Id = 2,
                    AthleteId = 2,
                    Status = JokerNutrition.Data.Enums.OnboardingAssessmentStatus.Reviewed,
                    SubmittedAt = submittedAt.AddMinutes(10),
                    ReviewedAt = submittedAt.AddHours(1),
                    CreatedAt = submittedAt,
                    UpdatedAt = submittedAt.AddHours(1)
                },
                new AthleteOnboardingAssessment
                {
                    Id = 3,
                    AthleteId = 3,
                    Status = JokerNutrition.Data.Enums.OnboardingAssessmentStatus.Submitted,
                    SubmittedAt = submittedAt.AddMinutes(20),
                    CreatedAt = submittedAt,
                    UpdatedAt = submittedAt.AddMinutes(20)
                });
        }

        if (!db.ClientCheckIns.Any())
        {
            var weekOf = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-7);
            db.ClientCheckIns.AddRange(
                new ClientCheckIn
                {
                    Id = 1,
                    AthleteId = 1,
                    WeekOf = weekOf,
                    SubmittedAt = DateTime.UtcNow.AddDays(-7),
                    WeightKg = 82.5m,
                    WaistCm = 88m,
                    ChestCm = 102m,
                    ThighCm = 60m,
                    SleepQuality = 8,
                    EnergyLevel = 7,
                    GutHealth = 9,
                    TrainingStress = 6,
                    CoachNotes = "Private review note",
                    CoachReviewedAt = DateTime.UtcNow.AddDays(-6)
                },
                new ClientCheckIn
                {
                    Id = 2,
                    AthleteId = 3,
                    WeekOf = weekOf,
                    SubmittedAt = DateTime.UtcNow.AddDays(-7),
                    WeightKg = 75m,
                    SleepQuality = 7,
                    EnergyLevel = 7,
                    GutHealth = 7,
                    TrainingStress = 7
                });
            db.CheckInPhotos.AddRange(
                new CheckInPhoto
                {
                    Id = 1,
                    ClientCheckInId = 2,
                    Angle = JokerNutrition.Data.Enums.PhotoAngle.Front,
                    BlobUrl = "http://127.0.0.1:10000/devstoreaccount1/athlete-progress-photos/checkins/3/front.jpg",
                    UploadedAt = DateTime.UtcNow.AddDays(-7)
                },
                new CheckInPhoto
                {
                    Id = 2,
                    ClientCheckInId = 1,
                    Angle = JokerNutrition.Data.Enums.PhotoAngle.Front,
                    BlobUrl = "http://127.0.0.1:8765/uploads/sample-progress.png",
                    UploadedAt = DateTime.UtcNow.AddDays(-7)
                });
        }

        db.SaveChanges();
    }
}

