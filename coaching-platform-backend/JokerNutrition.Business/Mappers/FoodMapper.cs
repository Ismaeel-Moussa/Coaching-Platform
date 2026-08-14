using JokerNutrition.Business.DTOs.Foods;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class FoodMapper
{
    public static FoodDto Map(Food food, bool isFavorite = false) => new()
    {
        Id = food.Id,
        Name = food.Name,
        Category = food.Category,
        CaloriesPer100g = food.CaloriesPer100g,
        ProteinPer100g = food.ProteinPer100g,
        CarbsPer100g = food.CarbsPer100g,
        FatPer100g = food.FatPer100g,
        FiberPer100g = food.FiberPer100g,
        IsCustom = food.IsCustom,
        IsFavorite = isFavorite
    };
}
