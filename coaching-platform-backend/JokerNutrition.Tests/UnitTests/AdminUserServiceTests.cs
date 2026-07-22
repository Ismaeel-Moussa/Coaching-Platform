using System.Security.Claims;
using JokerNutrition.Business.DTOs.Admin;
using JokerNutrition.Business.Services;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class AdminUserServiceTests
{
    private readonly DbContextOptions<JokerNutritionContext> _dbOptions;

    public AdminUserServiceTests()
    {
        _dbOptions = new DbContextOptionsBuilder<JokerNutritionContext>()
            .UseInMemoryDatabase(databaseName: $"JokerAdminTests_{Guid.NewGuid():N}")
            .Options;
    }

    [Fact]
    public async Task ToggleUserStatusAsync_PreventsSelfDeactivation()
    {
        using var context = new JokerNutritionContext(_dbOptions);
        var adminId = 100;

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Email, "admin@jokernutrition.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.GivenName, "AdminUser")
        }));

        var userStore = new FakeUserStore();
        var userManager = new FakeUserManager(userStore);

        var service = new AdminUserService(
            principal,
            NullLogger<AdminUserService>.Instance,
            userManager,
            context,
            new FakeAuditLogRepository(context),
            new FakePasswordResetTokenRepository(context),
            new FakeAthleteRepository(context),
            new FakeCoachRepository(context),
            new FakeNotificationService(),
            new FakeEmailService(),
            new FakeAuditLogService());

        var form = new ToggleUserStatusForm { IsActive = false, Reason = "Self test" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ToggleUserStatusAsync(adminId, form));

        Assert.Equal("You cannot deactivate your own admin account.", ex.Message);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_PreventsDeactivatingLastAdmin()
    {
        using var context = new JokerNutritionContext(_dbOptions);
        var adminRole = new Role { Id = 1, Name = "Admin" };
        var adminUser = new User { Id = 101, FirstName = "SoleAdmin", Email = "soleadmin@jokernutrition.com", IsActive = true };
        var userRole = new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id };

        context.Roles.Add(adminRole);
        context.Users.Add(adminUser);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "999"),
            new Claim(ClaimTypes.Email, "otheradmin@jokernutrition.com"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.GivenName, "OtherAdmin")
        }));

        var userStore = new FakeUserStore();
        var userManager = new FakeUserManager(userStore, isAdmin: true);

        var service = new AdminUserService(
            principal,
            NullLogger<AdminUserService>.Instance,
            userManager,
            context,
            new FakeAuditLogRepository(context),
            new FakePasswordResetTokenRepository(context),
            new FakeAthleteRepository(context),
            new FakeCoachRepository(context),
            new FakeNotificationService(),
            new FakeEmailService(),
            new FakeAuditLogService());

        var form = new ToggleUserStatusForm { IsActive = false, Reason = "Deactivating sole admin" };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ToggleUserStatusAsync(adminUser.Id, form));

        Assert.Equal("Cannot deactivate the last active Admin account in the system.", ex.Message);
    }

    private sealed class FakeUserStore : IUserStore<User>
    {
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
        public void Dispose() { }
        public Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<User?>(new User { Id = int.Parse(userId), FirstName = "Admin", Email = "admin@jokernutrition.com" });
        public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<User?>(null);
        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.UserName);
        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Id.ToString());
        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.UserName);
        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);
    }

    private sealed class FakeUserManager : UserManager<User>
    {
        private readonly bool _isAdmin;

        public FakeUserManager(IUserStore<User> store, bool isAdmin = false)
            : base(store, null!, null!, null!, null!, null!, null!, null!, null!)
        {
            _isAdmin = isAdmin;
        }

        public override Task<User?> FindByIdAsync(string userId) => Task.FromResult<User?>(new User { Id = int.Parse(userId), FirstName = "Test", Email = "test@user.com" });
        public override Task<IdentityResult> UpdateAsync(User user) => Task.FromResult(IdentityResult.Success);
        public override Task<bool> IsInRoleAsync(User user, string role) => Task.FromResult(role == "Admin" && _isAdmin);
    }

    private sealed class FakeAuditLogRepository : _BaseRepository<AuditLog>, IAuditLogRepository
    {
        public FakeAuditLogRepository(JokerNutritionContext context) : base(context, NullLogger<_BaseRepository<AuditLog>>.Instance) { }
    }

    private sealed class FakePasswordResetTokenRepository : _BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
    {
        public FakePasswordResetTokenRepository(JokerNutritionContext context) : base(context, NullLogger<_BaseRepository<PasswordResetToken>>.Instance) { }
    }

    private sealed class FakeAthleteRepository : _BaseRepository<Athlete>, IAthleteRepository
    {
        public FakeAthleteRepository(JokerNutritionContext context) : base(context, NullLogger<_BaseRepository<Athlete>>.Instance) { }
    }

    private sealed class FakeCoachRepository : _BaseRepository<Coach>, ICoachRepository
    {
        public FakeCoachRepository(JokerNutritionContext context) : base(context, NullLogger<_BaseRepository<Coach>>.Instance) { }
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public Task CreateAndSendNotificationAsync(int recipientUserId, JokerNutrition.Data.Enums.NotificationType type, string message) => Task.CompletedTask;
        public Task<List<JokerNutrition.Business.DTOs.Notifications.NotificationDto>> GetUserNotificationsAsync(bool? isRead = null) => Task.FromResult(new List<JokerNutrition.Business.DTOs.Notifications.NotificationDto>());
        public Task<int> GetUnreadCountAsync() => Task.FromResult(0);
        public Task MarkAsReadAsync(int notificationId) => Task.CompletedTask;
        public Task MarkAllAsReadAsync() => Task.CompletedTask;
        public Task SendDirectUpdateAsync(int recipientUserId, string method, object data) => Task.CompletedTask;
    }

    private sealed class FakeEmailService : IEmailService
    {
        public Task SendInvitationEmailAsync(string toEmail, string invitationUrl, string role) => Task.CompletedTask;
        public Task SendPasswordResetEmailAsync(string toEmail, string resetUrl) => Task.CompletedTask;
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public Task LogAsync(int? userId, string? performedByName, string action, string entityType, string? entityId = null, string? ipAddress = null, string? details = null) => Task.CompletedTask;
    }
}
