namespace JokerNutrition.Business.DTOs.Workouts;

public class WorkoutDayDto
{
    public int DayNumber { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public bool IsRestDay { get; set; }
    public List<TemplateExerciseDto> WarmUp { get; set; } = new();
    public List<TemplateExerciseDto> Main { get; set; } = new();
    public List<TemplateExerciseDto> CoolDown { get; set; } = new();
}
