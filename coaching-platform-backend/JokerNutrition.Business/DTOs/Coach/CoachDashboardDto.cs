namespace JokerNutrition.Business.DTOs.Coach;

public class CoachDashboardDto
{
    public int ActiveAthleteCount { get; set; }
    public double AvgWorkoutCompletionPercent { get; set; }
    public int PendingCheckInsCount { get; set; }
    public int PendingOnboardingAssessmentsCount { get; set; }
    public int AthletesNeedingSetupCount { get; set; }
    public List<CoachActionItemDto> ActionItems { get; set; } = new();
    public List<LiveFeedItemDto> RecentFeed { get; set; } = new();
}
