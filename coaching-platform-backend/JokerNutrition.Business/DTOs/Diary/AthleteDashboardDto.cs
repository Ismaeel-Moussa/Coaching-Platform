using JokerNutrition.Business.DTOs.Coach;
using System.Collections.Generic;

namespace JokerNutrition.Business.DTOs.Diary;

public class AthleteDashboardDto
{
    public AthleteInfoDto Athlete { get; set; } = null!;
    public MacroSummaryDto Today { get; set; } = null!;

    /// <summary>
    /// "NoProgram" | "Completed" | "InProgress" | "Rest"
    /// </summary>
    public string TodaysWorkoutStatus { get; set; } = "NoProgram";

    public List<CoachFeedbackNoteDto> RecentFeedbackNotes { get; set; } = new();
}

public class AthleteInfoDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public string? TargetGoal { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
