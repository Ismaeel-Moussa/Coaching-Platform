using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class ExerciseMapper
{
    public static ExerciseDto Map(Exercise e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Instructions = e.Instructions,
        PrimaryMuscle = e.PrimaryMuscle.ToString(),
        EquipmentRequired = e.EquipmentRequired,
        YouTubeVideoId = e.YouTubeVideoId
    };
}
