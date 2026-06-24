namespace JokerNutrition.Business.DTOs.Workouts;

public class TemplateExerciseDto
{
    public int Id { get; set; }
    public ExerciseDto Exercise { get; set; } = null!;
    public string Section { get; set; } = string.Empty;          // "WarmUp" | "Main" | "CoolDown"
    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public string TargetReps { get; set; } = string.Empty;       // e.g. "8-12"
    public int? RestSeconds { get; set; }
    public bool IsSupersetWith { get; set; }
    public decimal? ProgressiveOverloadTargetKg { get; set; }
}
