namespace JokerNutrition.Business.DTOs.Workouts;

public class WorkoutHistoryDto
{
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public List<ExerciseSessionDto> Sessions { get; set; } = new();
}

public class ExerciseSessionDto
{
    public DateOnly Date { get; set; }
    public List<SetLogDto> Sets { get; set; } = new();
}
