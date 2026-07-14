using JokerNutrition.Business.DTOs.Recipes;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class RecipeMapper
{
    public static RecipeDto Map(Recipe recipe, bool isFavorite = false) => new()
    {
        Id = recipe.Id,
        Name = recipe.Name,
        Description = recipe.Description,
        Category = recipe.Category,
        PrepTimeMinutes = recipe.PrepTimeMinutes,
        CookTimeMinutes = recipe.CookTimeMinutes,
        Servings = recipe.Servings,
        IsJokerRecipe = recipe.IsJokerRecipe,
        TotalCalories = recipe.TotalCalories,
        TotalProtein = recipe.TotalProtein,
        TotalCarbs = recipe.TotalCarbs,
        TotalFat = recipe.TotalFat,
        CreatedAt = recipe.CreatedAt,
        ImageUrl = recipe.ImageUrl,
        VideoUrl = recipe.VideoUrl,
        Ingredients = recipe.Ingredients.Select(MapIngredient).ToList(),
        IsFavorite = isFavorite
    };

    public static RecipeIngredientDto MapIngredient(RecipeIngredient ingredient)
    {
        var (cal, pro, carb, fat) = MacroCalculatorHelper.Calculate(
            ingredient.Food, ingredient.QuantityGrams);

        return new RecipeIngredientDto
        {
            FoodId = ingredient.FoodId,
            FoodName = ingredient.Food?.Name ?? string.Empty,
            QuantityGrams = ingredient.QuantityGrams,
            Calories = cal,
            Protein = pro,
            Carbs = carb,
            Fat = fat
        };
    }
}
