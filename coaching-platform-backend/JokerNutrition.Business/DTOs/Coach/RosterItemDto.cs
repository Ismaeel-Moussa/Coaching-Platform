namespace JokerNutrition.Business.DTOs.Coach;

public class RosterItemDto
{
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteAvatarUrl { get; set; }
    public string? ActiveProgramName { get; set; }
    public double MacroCompliancePercent { get; set; }
    public DateTime? LastCheckInDate { get; set; }

    /// <summary>"Active" | "ComplianceAlert" | "NoRecentCheckIn"</summary>
    public string Status { get; set; } = string.Empty;
}
