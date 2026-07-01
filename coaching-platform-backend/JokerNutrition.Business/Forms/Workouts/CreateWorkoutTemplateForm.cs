using System.ComponentModel.DataAnnotations;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Workouts;

public class CreateWorkoutTemplateForm
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MinLength(1)]
    public List<CreateWorkoutTemplateDayForm> Days { get; set; } = new();
}

public class CreateWorkoutTemplateDayForm
{
    [Range(1, 7)]
    public int DayNumber { get; set; }

    [Required, MaxLength(100)]
    public string DayLabel { get; set; } = string.Empty;

    public bool IsRestDay { get; set; }

    public List<CreateTemplateExerciseForm> Exercises { get; set; } = new();
}

public class CreateTemplateExerciseForm
{
    public int ExerciseId { get; set; }

    public ExerciseSection Section { get; set; }   // WarmUp, Main, CoolDown

    public int OrderIndex { get; set; }

    [Range(1, 100)]
    public int TargetSets { get; set; }

    [Required, MaxLength(20)]
    public string TargetReps { get; set; } = string.Empty;   // e.g. "8-12" or "15"

    public int? RestSeconds { get; set; }

    public decimal? ProgressiveOverloadTargetKg { get; set; }
}
