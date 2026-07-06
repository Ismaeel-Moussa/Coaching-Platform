using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JokerNutrition.Business.BackgroundServices;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the TokenCleanupHostedService purge rules.
/// </summary>
public class TokenCleanupTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;

    public TokenCleanupTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TokenCleanup_PurgesOnlyExpiredOrUsedTokens()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();

        // Clear existing tokens to ensure an isolated test run
        db.PasswordResetTokens.RemoveRange(db.PasswordResetTokens);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // 1. Valid Active Token (should NOT be deleted)
        var activeToken = new PasswordResetToken
        {
            UserId = 1, // Seeded coach user ID
            Token = "active-token-xyz",
            TokenType = "RefreshToken",
            ExpiresAt = now.AddDays(1),
            IsUsed = false,
            CreatedAt = now
        };

        // 2. Expired Token (should be deleted)
        var expiredToken = new PasswordResetToken
        {
            UserId = 1,
            Token = "expired-token-xyz",
            TokenType = "RefreshToken",
            ExpiresAt = now.AddMinutes(-5),
            IsUsed = false,
            CreatedAt = now.AddDays(-1)
        };

        // 3. Used Token (should be deleted even if not expired)
        var usedToken = new PasswordResetToken
        {
            UserId = 1,
            Token = "used-token-xyz",
            TokenType = "PasswordReset",
            ExpiresAt = now.AddDays(1),
            IsUsed = true,
            CreatedAt = now
        };

        db.PasswordResetTokens.AddRange(activeToken, expiredToken, usedToken);
        await db.SaveChangesAsync();

        // Construct hosted service directly using test host scope factory
        var scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        var logger = _factory.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TokenCleanupHostedService>>();
        var cleanupService = new TokenCleanupHostedService(scopeFactory, logger);

        // Act
        await cleanupService.PurgeExpiredTokensAsync(CancellationToken.None);

        // Assert
        var remainingTokens = await db.PasswordResetTokens.ToListAsync();

        Assert.Single(remainingTokens);
        Assert.Equal("active-token-xyz", remainingTokens[0].Token);
        Assert.False(remainingTokens[0].IsUsed);
    }
}
