namespace JokerNutrition.Business.DTOs.Coach;

public class LiveFeedItemDto
{
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteAvatarUrl { get; set; }
    public string WorkoutDayLabel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "InProgress" | "Completed" | "Missed"
    public DateTime? CompletedAt { get; set; }
    public DateOnly Date { get; set; }
}
