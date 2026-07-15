using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.NutritionPlans;

public class NutritionPlanSummaryDto
{
    public int Id { get; set; }
    public string SeedKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public decimal TargetCalories { get; set; }
    public decimal MinimumProteinGrams { get; set; }
    public ContentStatus ContentStatus { get; set; }
    public int ContentVersion { get; set; }
    public int MealBlockCount { get; set; }
    public decimal MealBlockCalories { get; set; }
    public decimal TrainingDayCalories { get; set; }
    public decimal RestDayCalories { get; set; }
    public int ActiveAssignmentCount { get; set; }
    public bool IsManuallyEdited { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class NutritionPlanDto : NutritionPlanSummaryDto
{
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public List<NutritionMealBlockDto> MealBlocks { get; set; } = new();
    public List<NutritionPlanRuleDto> Rules { get; set; } = new();
}

public class NutritionMealBlockDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public MealType MealType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public decimal? TargetCalories { get; set; }
    public bool TrainingDayOnly { get; set; }
    public bool RestDayOnly { get; set; }
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }
    public List<NutritionMealOptionDto> Options { get; set; } = new();
}

public class NutritionMealOptionDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public bool IsCompleteOption { get; set; }
    public List<NutritionOptionItemDto> Items { get; set; } = new();
}

public class NutritionOptionItemDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public int? FoodId { get; set; }
    public string? FoodName { get; set; }
    public string? FoodNameAr { get; set; }
    public int? RecipeId { get; set; }
    public string? RecipeName { get; set; }
    public string? RecipeNameAr { get; set; }
    public string? ItemName { get; set; }
    public string? ItemNameAr { get; set; }
    public decimal Quantity { get; set; }
    public IngredientUnit Unit { get; set; }
    public FoodPreparationState MeasurementState { get; set; }
    public string? AlternativeGroupKey { get; set; }
    public int? CatalogContentVersion { get; set; }
    public decimal? CaloriesPer100Grams { get; set; }
    public decimal? ProteinPer100Grams { get; set; }
    public decimal? CarbsPer100Grams { get; set; }
    public decimal? FatPer100Grams { get; set; }
    public decimal? RecipeTotalWeightGrams { get; set; }
    public decimal? RecipeTotalCalories { get; set; }
    public decimal? RecipeTotalProtein { get; set; }
    public decimal? RecipeTotalCarbs { get; set; }
    public decimal? RecipeTotalFat { get; set; }
}

public class NutritionPlanRuleDto
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string TextAr { get; set; } = string.Empty;
}

public class NutritionPlanValidationDto
{
    public bool IsValidForPublish => Issues.All(issue => issue.Severity != "Error");
    public decimal TargetCalories { get; set; }
    public decimal MealBlockCalories { get; set; }
    public decimal TrainingDayCalories { get; set; }
    public decimal RestDayCalories { get; set; }
    public List<NutritionPlanValidationIssueDto> Issues { get; set; } = new();
}

public class NutritionPlanValidationIssueDto
{
    public string Severity { get; set; } = "Error";
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Path { get; set; }
}

public class NutritionPlanAssignmentDto
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? TemplateNameAr { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime AssignedAt { get; set; }
    public NutritionPlanDto Plan { get; set; } = new();
}
