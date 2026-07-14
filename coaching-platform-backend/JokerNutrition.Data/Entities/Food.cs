using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class Food
{
    public int Id { get; set; }
    public string? SeedKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Category { get; set; }
    public bool IsCustom { get; set; } = false;
    public FoodPreparationState PreparationState { get; set; } = FoodPreparationState.Unspecified;
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Published;
    public int ContentVersion { get; set; } = 1;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }
    public decimal FiberPer100g { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
