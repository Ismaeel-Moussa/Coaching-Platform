using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Mappers;

public static class WorkoutMapper
{
    public static TemplateExerciseDto MapTemplateExercise(TemplateExercise te) => new()
    {
        Id = te.Id,
        Exercise = ExerciseMapper.Map(te.Exercise),
        Section = te.Section.ToString(),
        OrderIndex = te.OrderIndex,
        TargetSets = te.TargetSets,
        TargetReps = te.TargetReps,
        RestSeconds = te.RestSeconds,
        IsSupersetWith = te.IsSupersetWith,
        ProgressiveOverloadTargetKg = te.ProgressiveOverloadTargetKg
    };

    public static WorkoutDayDto MapDay(WorkoutTemplateDay day) => new()
    {
        DayNumber = day.DayNumber,
        DayLabel = day.DayLabel,
        IsRestDay = day.IsRestDay,
        WarmUp = day.Exercises
            .Where(e => e.Section == ExerciseSection.WarmUp)
            .OrderBy(e => e.OrderIndex)
            .Select(MapTemplateExercise)
            .ToList(),
        Main = day.Exercises
            .Where(e => e.Section == ExerciseSection.Main)
            .OrderBy(e => e.OrderIndex)
            .Select(MapTemplateExercise)
            .ToList(),
        CoolDown = day.Exercises
            .Where(e => e.Section == ExerciseSection.CoolDown)
            .OrderBy(e => e.OrderIndex)
            .Select(MapTemplateExercise)
            .ToList()
    };

    public static SetLogDto MapSet(ExerciseSetLog s) => new()
    {
        Id = s.Id,
        ExerciseId = s.ExerciseId,
        ExerciseName = s.Exercise?.Name ?? string.Empty,
        SetNumber = s.SetNumber,
        WeightKg = s.WeightKg,
        Reps = s.Reps,
        IsCompleted = s.IsCompleted
    };
}
