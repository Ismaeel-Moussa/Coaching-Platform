namespace JokerNutrition.Business.DTOs.Workouts;

public class WorkoutProgramDto
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public List<WorkoutDayDto> Days { get; set; } = new();
}
