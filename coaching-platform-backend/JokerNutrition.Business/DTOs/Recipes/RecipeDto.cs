using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Recipes;

public class RecipeDto
{
    public int Id { get; set; }
    public string? SeedKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? UsageNotes { get; set; }
    public string? UsageNotesAr { get; set; }
    public RecipeCategory Category { get; set; }
    public ContentStatus ContentStatus { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public bool IsJokerRecipe { get; set; }
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }
    public decimal? DeclaredCalories { get; set; }
    public decimal? DeclaredProtein { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    public List<RecipeStepDto> Steps { get; set; } = new();
    public bool IsFavorite { get; set; }
}

public class RecipeIngredientDto
{
    public int FoodId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public decimal QuantityGrams { get; set; }
    public decimal? DisplayQuantity { get; set; }
    public IngredientUnit Unit { get; set; }
    public FoodPreparationState MeasurementState { get; set; }
    public string? DisplayText { get; set; }
    public string? DisplayTextAr { get; set; }
    public bool IsOptional { get; set; }
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
}

public class RecipeStepDto
{
    public int OrderIndex { get; set; }
    public string? Instruction { get; set; }
    public string InstructionAr { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
}
