using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public MuscleGroup PrimaryMuscle { get; set; }
    public string? EquipmentRequired { get; set; }
    public string? YouTubeVideoId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<TemplateExercise> TemplateExercises { get; set; } = new List<TemplateExercise>();
}
