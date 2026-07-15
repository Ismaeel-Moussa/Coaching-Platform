namespace JokerNutrition.Business.DTOs.Workouts;

public class WorkoutTemplateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CoachName { get; set; } = string.Empty;
    public string ContentStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WorkoutTemplateDayDto> Days { get; set; } = new();
}

public class WorkoutTemplateDayDto
{
    public int Id { get; set; }
    public int DayNumber { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public bool IsRestDay { get; set; }
    public List<TemplateExerciseDto> Exercises { get; set; } = new();
}
