namespace JokerNutrition.Business.DTOs.Workouts;

public class WorkoutTemplateSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public int DayCount { get; set; }
    public string ContentStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
