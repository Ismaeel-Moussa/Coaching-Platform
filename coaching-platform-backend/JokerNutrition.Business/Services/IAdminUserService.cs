using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Admin;

namespace JokerNutrition.Business.Services;

public interface IAdminUserService
{
    Task<PagedResult<UserManagementDto>> GetUsersAsync(UserFilterParams filterParams);
    Task<UserMonitoringSummaryDto> GetMonitoringSummaryAsync();
    Task<CoachDeactivationImpactDto> GetCoachDeactivationImpactAsync(int coachUserId);
    Task ToggleUserStatusAsync(int targetUserId, ToggleUserStatusForm form);
    Task<List<UserLoginAuditLogDto>> GetUserLoginAuditLogsAsync(int userId);
    Task<byte[]> ExportUserAuditLogsCsvAsync(UserFilterParams filterParams);
}
