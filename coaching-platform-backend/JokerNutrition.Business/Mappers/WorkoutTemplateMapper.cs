using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class WorkoutTemplateMapper
{
    public static WorkoutTemplateSummaryDto MapSummary(WorkoutTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        CoachName = t.CreatedByCoach != null
            ? $"{t.CreatedByCoach.User.FirstName} {t.CreatedByCoach.User.LastName}"
            : string.Empty,
        DayCount = t.Days.Count,
        ContentStatus = t.ContentStatus.ToString(),
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt
    };

    public static WorkoutTemplateDto MapFull(WorkoutTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Description = t.Description,
        CoachName = t.CreatedByCoach != null
            ? $"{t.CreatedByCoach.User.FirstName} {t.CreatedByCoach.User.LastName}"
            : string.Empty,
        ContentStatus = t.ContentStatus.ToString(),
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt,
        Days = t.Days
            .OrderBy(d => d.DayNumber)
            .Select(MapDay)
            .ToList()
    };

    public static WorkoutTemplateDayDto MapDay(WorkoutTemplateDay day) => new()
    {
        Id = day.Id,
        DayNumber = day.DayNumber,
        DayLabel = day.DayLabel,
        IsRestDay = day.IsRestDay,
        Exercises = day.Exercises
            .OrderBy(e => e.Section)
            .ThenBy(e => e.OrderIndex)
            .Select(MapTemplateExercise)
            .ToList()
    };

    public static TemplateExerciseDto MapTemplateExercise(TemplateExercise te) => new()
    {
        Id = te.Id,
        Exercise = te.Exercise != null ? ExerciseMapper.Map(te.Exercise) : null!,
        Section = te.Section.ToString(),
        OrderIndex = te.OrderIndex,
        TargetSets = te.TargetSets,
        TargetReps = te.TargetReps,
        RestSeconds = te.RestSeconds,
        IsSupersetWith = te.IsSupersetWith,
        ProgressiveOverloadTargetKg = te.ProgressiveOverloadTargetKg
    };
}
