using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Athletes;

public class NutritionPlanAdherenceDto
{
    public int AssignmentId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string DayType { get; set; } = "AllDays";
    public bool IsPartialDay { get; set; }
    public int CompletedBlocks { get; set; }
    public int TotalBlocks { get; set; }
    public decimal CompletionPercent { get; set; }
    public List<NutritionPlanBlockAdherenceDto> Blocks { get; set; } = new();
}

public class NutritionPlanBlockAdherenceDto
{
    public int MealBlockId { get; set; }
    public int OrderIndex { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal? TargetCalories { get; set; }
    public string Status { get; set; } = "Pending";
    public int? MealOptionId { get; set; }
    public string? OptionLabel { get; set; }
    public MealType? LoggedMealType { get; set; }
    public decimal? Servings { get; set; }
    public DateTime? LoggedAt { get; set; }
}
