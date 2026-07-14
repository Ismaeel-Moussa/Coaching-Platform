using System;

namespace JokerNutrition.Data.Entities;

public class FavoriteRecipe
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
