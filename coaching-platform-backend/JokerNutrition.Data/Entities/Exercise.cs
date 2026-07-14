using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string? SeedKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }
    public MuscleGroup PrimaryMuscle { get; set; }
    public string? EquipmentRequired { get; set; }
    public string? YouTubeVideoId { get; set; }
    public string? VideoUrl { get; set; }
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Published;
    public int ContentVersion { get; set; } = 1;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<TemplateExercise> TemplateExercises { get; set; } = new List<TemplateExercise>();
}
