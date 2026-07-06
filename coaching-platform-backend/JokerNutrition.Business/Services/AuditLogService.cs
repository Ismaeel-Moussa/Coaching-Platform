using System.Security.Principal;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

// ─── Interface ────────────────────────────────────────────────────────────────
public interface IAuditLogService
{
    /// <summary>
    /// Fire-and-forget audit record. Action strings follow the convention:
    /// "Login", "MacroTargetSet", "CheckInPhotoAccess", "FoodDeleted", "ExerciseDeleted".
    /// </summary>
    Task LogAsync(
        int? userId,
        string? performedByName,
        string action,
        string entityType,
        string? entityId = null,
        string? ipAddress = null,
        string? details = null);
}

// ─── Service ──────────────────────────────────────────────────────────────────
public class AuditLogService : _BaseService, IAuditLogService
{
    private readonly IAuditLogRepository _auditRepo;

    public AuditLogService(
        IPrincipal principal,
        ILogger<AuditLogService> logger,
        IAuditLogRepository auditRepo)
        : base(principal, logger)
    {
        _auditRepo = auditRepo;
    }

    public async Task LogAsync(
        int? userId,
        string? performedByName,
        string action,
        string entityType,
        string? entityId = null,
        string? ipAddress = null,
        string? details = null)
    {
        try
        {
            var entry = new AuditLog
            {
                UserId = userId,
                PerformedByName = performedByName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                IpAddress = ipAddress,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await _auditRepo.CreateAsync(entry);
            await _auditRepo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit failures must NEVER break the main operation
            _logger.LogWarning(ex, "AuditLog write failed for action={Action} entityType={EntityType}", action, entityType);
        }
    }
}
