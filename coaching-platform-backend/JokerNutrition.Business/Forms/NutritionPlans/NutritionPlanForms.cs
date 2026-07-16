using System.ComponentModel.DataAnnotations;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.NutritionPlans;

public class UpsertNutritionPlanForm
{
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [Range(1, 20000)] public decimal TargetCalories { get; set; }
    [Range(1, 1000)] public decimal MinimumProteinGrams { get; set; }
    public int? ExpectedContentVersion { get; set; }
    public List<NutritionMealBlockForm> MealBlocks { get; set; } = new();
    public List<NutritionPlanRuleForm> Rules { get; set; } = new();
}

public class NutritionMealBlockForm
{
    [EnumDataType(typeof(MealType))]
    public MealType MealType { get; set; }
    [Required, MaxLength(200)] public string Label { get; set; } = string.Empty;
    [Range(0, 20000)] public decimal? TargetCalories { get; set; }
    public bool TrainingDayOnly { get; set; }
    public bool RestDayOnly { get; set; }
    [MaxLength(2000)] public string? Instructions { get; set; }
    public List<NutritionMealOptionForm> Options { get; set; } = new();
}

public class NutritionMealOptionForm
{
    [Required, MaxLength(200)] public string Label { get; set; } = string.Empty;
    public bool IsCompleteOption { get; set; } = true;
    public List<NutritionOptionItemForm> Items { get; set; } = new();
}

public class NutritionOptionItemForm
{
    public int? FoodId { get; set; }
    public int? RecipeId { get; set; }
    [MaxLength(300)] public string? ItemName { get; set; }
    [Range(0.01, 100000)] public decimal Quantity { get; set; }
    [EnumDataType(typeof(IngredientUnit))]
    public IngredientUnit Unit { get; set; } = IngredientUnit.Gram;
    [EnumDataType(typeof(FoodPreparationState))]
    public FoodPreparationState MeasurementState { get; set; }
    [MaxLength(100)] public string? AlternativeGroupKey { get; set; }
}

public class NutritionPlanRuleForm
{
    [Required, MaxLength(100)] public string RuleType { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Text { get; set; }
}

public class ChangeNutritionPlanStatusForm
{
    [EnumDataType(typeof(ContentStatus))]
    public ContentStatus Status { get; set; }
    [Range(1, int.MaxValue)] public int ExpectedContentVersion { get; set; }
}

public class AssignNutritionPlanForm
{
    [Required, MinLength(1)] public List<int> AthleteIds { get; set; } = new();
    public DateTime? StartDate { get; set; }
    [MaxLength(2000)] public string? Notes { get; set; }
}
