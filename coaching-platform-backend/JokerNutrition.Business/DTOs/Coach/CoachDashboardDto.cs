namespace JokerNutrition.Business.DTOs.Coach;

public class CoachDashboardDto
{
    public int ActiveAthleteCount { get; set; }
    public double AvgWorkoutCompletionPercent { get; set; }
    public int PendingCheckInsCount { get; set; }
    public List<LiveFeedItemDto> RecentFeed { get; set; } = new();
}
