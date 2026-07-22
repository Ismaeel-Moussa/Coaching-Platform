using System.Security.Principal;
using System.Text;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Admin;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public class AdminUserService : _BaseService, IAdminUserService
{
    private readonly UserManager<User> _userManager;
    private readonly JokerNutritionContext _context;
    private readonly IAuditLogRepository _auditRepo;
    private readonly IPasswordResetTokenRepository _tokenRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IAuditLogService _auditLogService;

    public AdminUserService(
        IPrincipal principal,
        ILogger<AdminUserService> logger,
        UserManager<User> userManager,
        JokerNutritionContext context,
        IAuditLogRepository auditRepo,
        IPasswordResetTokenRepository tokenRepo,
        IAthleteRepository athleteRepo,
        ICoachRepository coachRepo,
        INotificationService notificationService,
        IEmailService emailService,
        IAuditLogService auditLogService)
        : base(principal, logger)
    {
        _userManager = userManager;
        _context = context;
        _auditRepo = auditRepo;
        _tokenRepo = tokenRepo;
        _athleteRepo = athleteRepo;
        _coachRepo = coachRepo;
        _notificationService = notificationService;
        _emailService = emailService;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResult<UserManagementDto>> GetUsersAsync(UserFilterParams filterParams)
    {
        var page = filterParams.PageNumber <= 0 ? 1 : filterParams.PageNumber;
        var pageSize = filterParams.PageSize <= 0 ? 10 : filterParams.PageSize;

        // Base user query with roles
        var query = from u in _context.Users
                    join ur in _context.UserRoles on u.Id equals ur.UserId into urGroup
                    from ur in urGroup.DefaultIfEmpty()
                    join r in _context.Roles on ur.RoleId equals r.Id into rGroup
                    from r in rGroup.DefaultIfEmpty()
                    select new
                    {
                        User = u,
                        RoleName = r != null ? r.Name : "Athlete"
                    };

        // Search filter
        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            query = query.Where(x => x.User.FirstName.ToLower().Contains(search)
                                  || x.User.LastName.ToLower().Contains(search)
                                  || (x.User.Email != null && x.User.Email.ToLower().Contains(search)));
        }

        // Role filter
        if (!string.IsNullOrWhiteSpace(filterParams.Role) && !filterParams.Role.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.RoleName == filterParams.Role);
        }

        // Status filter
        if (filterParams.IsActive.HasValue)
        {
            query = query.Where(x => x.User.IsActive == filterParams.IsActive.Value);
        }

        // Inactivity filter
        if (!string.IsNullOrWhiteSpace(filterParams.InactivityFilter))
        {
            var filter = filterParams.InactivityFilter.ToLower();
            if (filter == "never")
            {
                query = query.Where(x => x.User.LastLoginAt == null);
            }
            else if (filter == "30days")
            {
                var cutoff = DateTime.UtcNow.AddDays(-30);
                query = query.Where(x => x.User.LastLoginAt == null || x.User.LastLoginAt < cutoff);
            }
            else if (filter == "24h")
            {
                var cutoff = DateTime.UtcNow.AddDays(-1);
                query = query.Where(x => x.User.LastLoginAt >= cutoff);
            }
        }

        var totalCount = await query.CountAsync();

        var pagedUsers = await query
            .OrderByDescending(x => x.User.LastLoginAt.HasValue)
            .ThenByDescending(x => x.User.LastLoginAt)
            .ThenByDescending(x => x.User.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userIds = pagedUsers.Select(x => x.User.Id).ToList();

        // Get athlete details (assigned coach info)
        var athleteMap = await _context.Athletes
            .Include(a => a.AssignedCoach)
            .ThenInclude(c => c.User)
            .Where(a => userIds.Contains(a.UserId))
            .ToDictionaryAsync(a => a.UserId, a => a);

        // Get coach details (assigned athlete counts)
        var coachMap = await _context.Coaches
            .Where(c => userIds.Contains(c.UserId))
            .Select(c => new
            {
                c.UserId,
                c.Id,
                AthleteCount = _context.Athletes.Count(a => a.AssignedCoachId == c.Id)
            })
            .ToDictionaryAsync(c => c.UserId, c => c.AthleteCount);

        var dtos = pagedUsers.Select(x =>
        {
            var u = x.User;
            var dto = new UserManagementDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                FirstName = u.FirstName,
                LastName = u.LastName,
                ProfilePictureUrl = u.ProfilePictureUrl,
                Role = x.RoleName ?? "Athlete",
                IsActive = u.IsActive,
                DeactivationReason = u.DeactivationReason,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                LastLoginIp = u.LastLoginIp
            };

            if (athleteMap.TryGetValue(u.Id, out var athlete) && athlete.AssignedCoach != null)
            {
                dto.AssignedCoachId = athlete.AssignedCoach.Id;
                dto.AssignedCoachName = $"{athlete.AssignedCoach.User.FirstName} {athlete.AssignedCoach.User.LastName}".Trim();
            }

            if (coachMap.TryGetValue(u.Id, out var count))
            {
                dto.AssignedAthleteCount = count;
            }

            return dto;
        }).ToList();

        return new PagedResult<UserManagementDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserMonitoringSummaryDto> GetMonitoringSummaryAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var deactivatedUsers = await _context.Users.CountAsync(u => !u.IsActive);
        
        var cutoff24h = DateTime.UtcNow.AddDays(-1);
        var activeLast24h = await _context.Users.CountAsync(u => u.LastLoginAt >= cutoff24h);

        // Active Coaches & Athletes by Role
        var activeCoaches = await (from u in _context.Users
                                   join ur in _context.UserRoles on u.Id equals ur.UserId
                                   join r in _context.Roles on ur.RoleId equals r.Id
                                   where u.IsActive && r.Name == "Coach"
                                   select u.Id).CountAsync();

        var activeAthletes = await (from u in _context.Users
                                    join ur in _context.UserRoles on u.Id equals ur.UserId
                                    join r in _context.Roles on ur.RoleId equals r.Id
                                    where u.IsActive && r.Name == "Athlete"
                                    select u.Id).CountAsync();

        return new UserMonitoringSummaryDto
        {
            TotalUsersCount = totalUsers,
            ActiveCoachesCount = activeCoaches,
            ActiveAthletesCount = activeAthletes,
            DeactivatedUsersCount = deactivatedUsers,
            ActiveLast24hCount = activeLast24h
        };
    }

    public async Task<CoachDeactivationImpactDto> GetCoachDeactivationImpactAsync(int coachUserId)
    {
        var coach = await _context.Coaches
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == coachUserId)
            ?? throw new KeyNotFoundException("Coach profile not found.");

        var assignedAthletesCount = await _context.Athletes.CountAsync(a => a.AssignedCoachId == coach.Id);
        var activeWorkoutPlans = await _context.WorkoutTemplates.CountAsync(w => w.CreatedByCoachId == coach.Id);
        var activeNutritionAssignments = await (from a in _context.NutritionPlanAssignments
                                                join ath in _context.Athletes on a.AthleteId equals ath.Id
                                                where ath.AssignedCoachId == coach.Id && a.IsActive
                                                select a.Id).CountAsync();

        return new CoachDeactivationImpactDto
        {
            CoachId = coach.Id,
            CoachName = $"{coach.User.FirstName} {coach.User.LastName}".Trim(),
            AssignedAthletesCount = assignedAthletesCount,
            ActiveNutritionPlansCount = activeNutritionAssignments,
            ActiveWorkoutPlansCount = activeWorkoutPlans
        };
    }

    public async Task ToggleUserStatusAsync(int targetUserId, ToggleUserStatusForm form)
    {
        if (targetUserId == LoggedInUser.Id && !form.IsActive)
        {
            throw new InvalidOperationException("You cannot deactivate your own admin account.");
        }

        var user = await _userManager.FindByIdAsync(targetUserId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        if (!form.IsActive)
        {
            var isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isTargetAdmin)
            {
                var activeAdminCount = await (from u in _context.Users
                                              join ur in _context.UserRoles on u.Id equals ur.UserId
                                              join r in _context.Roles on ur.RoleId equals r.Id
                                              where u.IsActive && r.Name == "Admin"
                                              select u.Id).CountAsync();
                if (activeAdminCount <= 1)
                {
                    throw new InvalidOperationException("Cannot deactivate the last active Admin account in the system.");
                }
            }
        }

        // If deactivating a user
        if (!form.IsActive)
        {
            // If user is a coach, handle assigned athletes
            var coach = await _context.Coaches.FirstOrDefaultAsync(c => c.UserId == targetUserId);
            if (coach != null)
            {
                var assignedAthletes = await _context.Athletes.Where(a => a.AssignedCoachId == coach.Id).ToListAsync();
                if (assignedAthletes.Any())
                {
                    if (form.ReassignCoachId.HasValue && form.ReassignCoachId.Value > 0)
                    {
                        var targetCoach = await _context.Coaches.FirstOrDefaultAsync(c => c.Id == form.ReassignCoachId.Value)
                            ?? throw new ArgumentException("Target reassignment coach not found.");
                        
                        foreach (var athlete in assignedAthletes)
                        {
                            athlete.AssignedCoachId = targetCoach.Id;
                        }
                    }
                    else
                    {
                        foreach (var athlete in assignedAthletes)
                        {
                            athlete.AssignedCoachId = null;
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // Revoke active refresh tokens
            var activeTokens = await _tokenRepo.Query()
                .Where(t => t.UserId == targetUserId && t.TokenType == "RefreshToken" && !t.IsUsed)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsUsed = true;
                _tokenRepo.Update(token);
            }
            await _tokenRepo.SaveChangesAsync();

            // Broadcast real-time eviction event via SignalR
            await _notificationService.SendDirectUpdateAsync(targetUserId, "UserEvicted", new { reason = form.Reason });
            
            user.DeactivationReason = form.Reason;
        }
        else
        {
            user.DeactivationReason = null;
        }

        user.IsActive = form.IsActive;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", updateResult.Errors.Select(e => e.Description)));
        }

        // Audit log entry
        await _auditLogService.LogAsync(
            userId: LoggedInUser.Id,
            performedByName: $"{LoggedInUser.FirstName} ({LoggedInUser.Email})",
            action: form.IsActive ? "UserReactivated" : "UserDeactivated",
            entityType: "User",
            entityId: targetUserId.ToString(),
            ipAddress: null);

        // Optional email notification
        if (!string.IsNullOrEmpty(user.Email))
        {
            var subject = form.IsActive ? "Account Reactivated - JokerNutrition" : "Account Suspended - JokerNutrition";
            var body = form.IsActive
                ? $"Hello {user.FirstName}, your account has been reactivated. You can now log back into the platform."
                : $"Hello {user.FirstName}, your account has been suspended. Reason: {form.Reason ?? "Administrative decision"}. Please contact support for inquiries.";
            
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Email, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send status update email to {Email}", user.Email);
            }
        }
    }

    public async Task<List<UserLoginAuditLogDto>> GetUserLoginAuditLogsAsync(int userId)
    {
        var logs = await _auditRepo.Query()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync();

        return logs.Select(a => new UserLoginAuditLogDto
        {
            Id = a.Id,
            Action = a.Action,
            PerformedByName = a.PerformedByName,
            IpAddress = a.IpAddress,
            CreatedAt = a.CreatedAt,
            Details = a.Details
        }).ToList();
    }

    public async Task<byte[]> ExportUserAuditLogsCsvAsync(UserFilterParams filterParams)
    {
        var logsQuery = _auditRepo.Query();

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            logsQuery = logsQuery.Where(a => (a.Action != null && a.Action.ToLower().Contains(search))
                                          || (a.PerformedByName != null && a.PerformedByName.ToLower().Contains(search))
                                          || (a.Details != null && a.Details.ToLower().Contains(search))
                                          || (a.IpAddress != null && a.IpAddress.ToLower().Contains(search)));
        }

        var logs = await logsQuery
            .OrderByDescending(a => a.CreatedAt)
            .Take(5000)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("ID,Date/Time (UTC),Action,Performed By,User ID,IP Address,Details");

        foreach (var log in logs)
        {
            sb.AppendLine($"\"{log.Id}\",\"{log.CreatedAt:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(log.Action)}\",\"{EscapeCsv(log.PerformedByName)}\",\"{log.UserId}\",\"{EscapeCsv(log.IpAddress)}\",\"{EscapeCsv(log.Details)}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"");
    }
}
