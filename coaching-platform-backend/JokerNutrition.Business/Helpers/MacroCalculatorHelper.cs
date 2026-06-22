using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Helpers;

public static class MacroCalculatorHelper
{
    // Cooking conversion factors (raw weight to cooked equivalent)
    private static readonly Dictionary<string, decimal> CookedFactors = new()
    {
        { "chicken", 0.75m },
        { "beef", 0.75m },
        { "fish", 0.80m },
        { "default", 0.80m }
    };

    // Dry-to-cooked: oats, rice absorb water and increase in weight
    private static readonly decimal DryToCooked = 2.5m;

    /// <summary>
    /// Calculate macros for a food item given quantity in grams and cooking state.
    /// Returns (calories, protein, carbs, fat) tuple.
    /// </summary>
    public static (decimal Calories, decimal Protein, decimal Carbs, decimal Fat) Calculate(
        Food food,
        decimal quantityGrams,
        FoodState state)
    {
        // Normalize: convert to raw equivalent 100g basis
        decimal effectiveGrams = state switch
        {
            FoodState.Cooked => quantityGrams / CookedFactors.GetValueOrDefault("default"),
            FoodState.Dry => quantityGrams * DryToCooked,
            _ => quantityGrams // Raw — no conversion needed
        };

        var factor = effectiveGrams / 100m;

        return (
            Math.Round(food.CaloriesPer100g * factor, 1),
            Math.Round(food.ProteinPer100g * factor, 1),
            Math.Round(food.CarbsPer100g * factor, 1),
            Math.Round(food.FatPer100g * factor, 1)
        );
    }

    /// <summary>
    /// Calculates combined macros for all ingredients in a recipe and returns totals.
    /// </summary>
    public static (decimal Calories, decimal Protein, decimal Carbs, decimal Fat)
        CalculateRecipeTotals(IEnumerable<(Food Food, decimal QuantityGrams, FoodState State)> ingredients)
    {
        decimal cal = 0, pro = 0, carb = 0, fat = 0;
        foreach (var (food, qty, state) in ingredients)
        {
            var (c, p, cb, f) = Calculate(food, qty, state);
            cal += c; pro += p; carb += cb; fat += f;
        }
        return (Math.Round(cal, 1), Math.Round(pro, 1), Math.Round(carb, 1), Math.Round(fat, 1));
    }
}
