namespace JokerNutrition.Business.DTOs.Workouts;

public class TodaysWorkoutDto
{
    public int WorkoutLogId { get; set; }
    public string Status { get; set; } = string.Empty;           // "InProgress" | "Completed" | "Missed" | "NoProgram"
    public DateTime? CompletedAt { get; set; }
    public WorkoutDayDto? Day { get; set; }
    public List<SetLogDto> LoggedSets { get; set; } = new();
}
