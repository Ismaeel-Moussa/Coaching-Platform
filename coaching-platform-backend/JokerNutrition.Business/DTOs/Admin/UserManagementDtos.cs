namespace JokerNutrition.Business.DTOs.Admin;

public class UserManagementDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? DeactivationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int? AssignedCoachId { get; set; }
    public string? AssignedCoachName { get; set; }
    public int AssignedAthleteCount { get; set; }
}

public class UserMonitoringSummaryDto
{
    public int TotalUsersCount { get; set; }
    public int ActiveCoachesCount { get; set; }
    public int ActiveAthletesCount { get; set; }
    public int DeactivatedUsersCount { get; set; }
    public int ActiveLast24hCount { get; set; }
}

public class CoachDeactivationImpactDto
{
    public int CoachId { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public int AssignedAthletesCount { get; set; }
    public int ActiveNutritionPlansCount { get; set; }
    public int ActiveWorkoutPlansCount { get; set; }
}

public class ToggleUserStatusForm
{
    public bool IsActive { get; set; }
    public string? Reason { get; set; }
    public int? ReassignCoachId { get; set; }
}

public class UserFilterParams
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? InactivityFilter { get; set; } // "never", "30days", "24h"
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UserLoginAuditLogDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PerformedByName { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Details { get; set; }
}
