using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class WorkoutTemplate
{
    public int Id { get; set; }
    public string? SeedKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Guidance { get; set; }
    public string? GuidanceAr { get; set; }
    public int? DailyStepTarget { get; set; }
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Published;
    public int ContentVersion { get; set; } = 1;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public int CreatedByCoachId { get; set; }
    public Coach CreatedByCoach { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<WorkoutTemplateDay> Days { get; set; } = new List<WorkoutTemplateDay>();
    public ICollection<ClientProgram> ClientPrograms { get; set; } = new List<ClientProgram>();
}

public class WorkoutTemplateDay
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int DayNumber { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public string? DayLabelAr { get; set; }
    public bool IsRestDay { get; set; } = false;
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }
    public string? CardioInstructions { get; set; }
    public string? CardioInstructionsAr { get; set; }

    public ICollection<TemplateExercise> Exercises { get; set; } = new List<TemplateExercise>();
}

public class TemplateExercise
{
    public int Id { get; set; }
    public int WorkoutTemplateDayId { get; set; }
    public WorkoutTemplateDay Day { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public ExerciseSection Section { get; set; }
    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public string TargetReps { get; set; } = string.Empty;
    public int? RestSeconds { get; set; }
    public bool IsSupersetWith { get; set; } = false;
    public int? SupersetGroupId { get; set; }
    public decimal? ProgressiveOverloadTargetKg { get; set; }
    public decimal? TargetRir { get; set; }
    public string? CoachNotes { get; set; }
    public string? CoachNotesAr { get; set; }
    public string? AlternativeGroupKey { get; set; }
}
