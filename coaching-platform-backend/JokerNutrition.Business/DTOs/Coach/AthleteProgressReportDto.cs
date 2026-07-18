namespace JokerNutrition.Business.DTOs.Coach;

public class AthleteProgressReportDto
{
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? TargetGoal { get; set; }
    public decimal? HeightCm { get; set; }
    public string PeriodStart { get; set; } = string.Empty;
    public string PeriodEnd { get; set; } = string.Empty;
    public int Weeks { get; set; }
    public string GeneratedAt { get; set; } = string.Empty;
    public ProgressReportSummaryDto Summary { get; set; } = new();
    public List<ProgressReportWeekDto> WeeklyProgress { get; set; } = new();
    public List<ProgressReportCheckInDto> CheckIns { get; set; } = new();
    public List<ProgressReportNoteDto> CoachNotes { get; set; } = new();
    public List<ProgressReportPhotoDto> ProgressPhotos { get; set; } = new();
}

public class ProgressReportSummaryDto
{
    public decimal? StartingWeightKg { get; set; }
    public decimal? CurrentWeightKg { get; set; }
    public decimal? WeightChangeKg { get; set; }
    public int LoggedWorkouts { get; set; }
    public int CompletedWorkouts { get; set; }
    public double? WorkoutCompletionPercent { get; set; }
    public int NutritionTrackedDays { get; set; }
    public double? AverageCalorieAdherencePercent { get; set; }
    public double? AverageProteinAdherencePercent { get; set; }
    public double? AverageStepsAdherencePercent { get; set; }
    public int CheckInCount { get; set; }
}

public class ProgressReportWeekDto
{
    public string WeekOf { get; set; } = string.Empty;
    public decimal? WeightKg { get; set; }
    public int LoggedWorkouts { get; set; }
    public int CompletedWorkouts { get; set; }
    public double? WorkoutCompletionPercent { get; set; }
    public double? CalorieAdherencePercent { get; set; }
    public double? ProteinAdherencePercent { get; set; }
    public double? StepsAdherencePercent { get; set; }
    public bool CheckInSubmitted { get; set; }
}

public class ProgressReportCheckInDto
{
    public int Id { get; set; }
    public string WeekOf { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? ThighCm { get; set; }
    public int SleepQuality { get; set; }
    public int EnergyLevel { get; set; }
    public int GutHealth { get; set; }
    public int TrainingStress { get; set; }
    public string? ReviewNotes { get; set; }
    public string? ReviewedAt { get; set; }
}

public class ProgressReportNoteDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
}

public class ProgressReportPhotoDto
{
    public int Id { get; set; }
    public string WeekOf { get; set; } = string.Empty;
    public string Angle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
