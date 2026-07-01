using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Exercises;

public class CreateExerciseForm
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string PrimaryMuscle { get; set; } = string.Empty;   // MuscleGroup enum name

    [MaxLength(500)]
    public string? EquipmentRequired { get; set; }

    [MaxLength(4000)]
    public string? Instructions { get; set; }

    [MaxLength(20)]
    public string? YouTubeVideoId { get; set; }
}
