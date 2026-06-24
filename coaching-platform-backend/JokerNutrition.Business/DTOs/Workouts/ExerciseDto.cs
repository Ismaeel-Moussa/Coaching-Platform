namespace JokerNutrition.Business.DTOs.Workouts;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string PrimaryMuscle { get; set; } = string.Empty;   // Enum name string
    public string? EquipmentRequired { get; set; }
    public string? YouTubeVideoId { get; set; }
}
