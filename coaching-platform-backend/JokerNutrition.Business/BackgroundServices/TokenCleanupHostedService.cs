using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using JokerNutrition.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace JokerNutrition.Business.BackgroundServices;

/// <summary>
/// A periodic background service that purges expired or used security tokens
/// (Refresh tokens and Password reset tokens) from the database to prevent unbounded table growth.
/// </summary>
public class TokenCleanupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TokenCleanupHostedService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24);

    public TokenCleanupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<TokenCleanupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Hosted Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PurgeExpiredTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing token cleanup.");
            }

            // Wait 24 hours before next execution (respecting cancellation requests)
            await Task.Delay(_cleanupInterval, stoppingToken);
        }

        _logger.LogInformation("Token Cleanup Hosted Service stopped.");
    }

    public async Task PurgeExpiredTokensAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting database token cleanup purge...");

        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            var now = DateTime.UtcNow;

            // Find all tokens that are either marked as used or have passed their expiration date
            var expiredOrUsedTokens = await db.PasswordResetTokens
                .Where(t => t.IsUsed || t.ExpiresAt < now)
                .ToListAsync(cancellationToken);

            if (expiredOrUsedTokens.Any())
            {
                db.PasswordResetTokens.RemoveRange(expiredOrUsedTokens);
                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Purged {Count} stale or used security tokens from the database.", expiredOrUsedTokens.Count);
            }
            else
            {
                _logger.LogInformation("No stale security tokens found to purge.");
            }
        }
    }
}
