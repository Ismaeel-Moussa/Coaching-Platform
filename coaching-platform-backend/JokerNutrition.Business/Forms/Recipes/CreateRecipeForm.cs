using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Recipes;

public class CreateRecipeForm
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RecipeCategory Category { get; set; } = RecipeCategory.Custom;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public List<RecipeIngredientForm> Ingredients { get; set; } = new();
}

public class RecipeIngredientForm
{
    public int FoodId { get; set; }
    public decimal QuantityGrams { get; set; }
    public FoodState State { get; set; } = FoodState.Raw;
}
