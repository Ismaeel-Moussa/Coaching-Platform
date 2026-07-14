using JokerNutrition.Business.DTOs.Recipes;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class RecipeMapper
{
    public static RecipeDto Map(Recipe recipe, bool isFavorite = false) => new()
    {
        Id = recipe.Id,
        SeedKey = recipe.SeedKey,
        Name = recipe.Name,
        NameAr = recipe.NameAr,
        Description = recipe.Description,
        DescriptionAr = recipe.DescriptionAr,
        UsageNotes = recipe.UsageNotes,
        UsageNotesAr = recipe.UsageNotesAr,
        Category = recipe.Category,
        ContentStatus = recipe.ContentStatus,
        PrepTimeMinutes = recipe.PrepTimeMinutes,
        CookTimeMinutes = recipe.CookTimeMinutes,
        Servings = recipe.Servings,
        IsJokerRecipe = recipe.IsJokerRecipe,
        TotalCalories = recipe.TotalCalories,
        TotalProtein = recipe.TotalProtein,
        TotalCarbs = recipe.TotalCarbs,
        TotalFat = recipe.TotalFat,
        DeclaredCalories = recipe.DeclaredCalories,
        DeclaredProtein = recipe.DeclaredProtein,
        CreatedAt = recipe.CreatedAt,
        ImageUrl = recipe.ImageUrl,
        VideoUrl = recipe.VideoUrl,
        Ingredients = recipe.Ingredients.Select(MapIngredient).ToList(),
        Steps = recipe.Steps.OrderBy(step => step.OrderIndex).Select(step => new RecipeStepDto
        {
            OrderIndex = step.OrderIndex,
            Instruction = step.Instruction,
            InstructionAr = step.InstructionAr,
            MediaUrl = step.MediaUrl
        }).ToList(),
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
            DisplayQuantity = ingredient.DisplayQuantity,
            Unit = ingredient.Unit,
            MeasurementState = ingredient.MeasurementState,
            DisplayText = ingredient.DisplayText,
            DisplayTextAr = ingredient.DisplayTextAr,
            IsOptional = ingredient.IsOptional,
            Calories = cal,
            Protein = pro,
            Carbs = carb,
            Fat = fat
        };
    }
}
