using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Exercises;

/// <summary>All fields optional — only send what you want to change.</summary>
public class UpdateExerciseForm
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public string? PrimaryMuscle { get; set; }   // MuscleGroup enum name

    [MaxLength(500)]
    public string? EquipmentRequired { get; set; }

    [MaxLength(4000)]
    public string? Instructions { get; set; }

    [MaxLength(20)]
    public string? YouTubeVideoId { get; set; }
}
