using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RecipeCategory Category { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public bool IsJokerRecipe { get; set; } = false;
    public int? CreatedByAthleteId { get; set; }
    public Athlete? CreatedByAthlete { get; set; }
    public DateTime CreatedAt { get; set; }

    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
}

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int FoodId { get; set; }
    public Food Food { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
}
