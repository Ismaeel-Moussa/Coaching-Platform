using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Diary;

public class MealLogDto
{
    public int Id { get; set; }
    public MealType MealType { get; set; }
    public FoodSummaryDto? Food { get; set; }
    public RecipeSummaryDto? Recipe { get; set; }
    public decimal QuantityGrams { get; set; }
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public DateTime LoggedAt { get; set; }
}

public class FoodSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
}

public class RecipeSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
