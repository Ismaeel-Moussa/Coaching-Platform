namespace JokerNutrition.Business.DTOs.Coach;

public class CoachActionItemDto
{
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteAvatarUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int? ProgressCurrent { get; set; }
    public int? ProgressTotal { get; set; }
    public double? MetricValue { get; set; }
}
