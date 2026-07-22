using JokerNutrition.Api.Filters;
using JokerNutrition.Business.DTOs.Admin;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class AdminController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>
    /// Gets paginated, searchable, and filtered list of users for admin monitoring.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] UserFilterParams filterParams)
    {
        var result = await _adminUserService.GetUsersAsync(filterParams);
        return Ok(result);
    }

    /// <summary>
    /// Gets high-level system user monitoring summary metrics.
    /// </summary>
    [HttpGet("monitoring-summary")]
    public async Task<IActionResult> GetMonitoringSummary()
    {
        var result = await _adminUserService.GetMonitoringSummaryAsync();
        return Ok(result);
    }

    /// <summary>
    /// Calculates deactivation impact (assigned clients, active plans) before deactivating a coach.
    /// </summary>
    [HttpGet("coaches/{coachId:int}/deactivation-impact")]
    public async Task<IActionResult> GetCoachDeactivationImpact(int coachId)
    {
        var result = await _adminUserService.GetCoachDeactivationImpactAsync(coachId);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates or reactivates a user account, revokes active refresh tokens, and optional coach reassignment.
    /// </summary>
    [HttpPost("users/{userId:int}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int userId, [FromBody] ToggleUserStatusForm form)
    {
        await _adminUserService.ToggleUserStatusAsync(userId, form);
        return NoContent();
    }

    /// <summary>
    /// Fetches historical sign-in and security audit logs for a specific user.
    /// </summary>
    [HttpGet("users/{userId:int}/audit-logs")]
    public async Task<IActionResult> GetUserLoginAuditLogs(int userId)
    {
        var result = await _adminUserService.GetUserLoginAuditLogsAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Downloads CSV report of user sign-in and security audit activities.
    /// </summary>
    [HttpGet("users/export-audit-csv")]
    public async Task<IActionResult> ExportUserAuditLogsCsv([FromQuery] UserFilterParams filterParams)
    {
        var bytes = await _adminUserService.ExportUserAuditLogsCsvAsync(filterParams);
        return File(bytes, "text/csv", $"user_login_audit_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
