using JokerNutrition.Business.DTOs.Athletes;

namespace JokerNutrition.Business.DTOs.Coach;

public class AthleteDeepProfileDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? TargetGoal { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }

    /// <summary>The athlete's current active macro targets set by the coach.</summary>
    public MacroTargetDto? CurrentTargets { get; set; }
    public AthleteSetupReadinessDto SetupReadiness { get; set; } = new();

    /// <summary>Weight measurements from weekly check-ins, ordered oldest → newest.</summary>
    public List<WeightHistoryPointDto> WeightHistory { get; set; } = new();

    /// <summary>Coach notes written for this athlete, ordered newest → oldest.</summary>
    public List<CoachFeedbackNoteDto> FeedbackNotes { get; set; } = new();
}
