namespace JokerNutrition.Business.DTOs.Coach;

public class ComplianceItemDto
{
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public string? AthleteAvatarUrl { get; set; }

    // Calorie compliance
    public decimal TargetCalories { get; set; }
    public decimal ConsumedCalories { get; set; }

    // Macro compliance
    public decimal TargetProtein { get; set; }
    public decimal ConsumedProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal ConsumedCarbs { get; set; }
    public decimal TargetFat { get; set; }
    public decimal ConsumedFat { get; set; }

    /// <summary>True when consumed calories exceed target by more than 5%.</summary>
    public bool IsOverCalorieTarget { get; set; }

    /// <summary>0–100 score based on calorie proximity to target.</summary>
    public double CompliancePercent { get; set; }
}
