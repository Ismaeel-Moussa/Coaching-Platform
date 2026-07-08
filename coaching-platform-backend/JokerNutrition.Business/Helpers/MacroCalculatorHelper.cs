using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Helpers;

public static class MacroCalculatorHelper
{
    /// <summary>
    /// Calculate macros for a food item given quantity in grams.
    /// Returns (calories, protein, carbs, fat) tuple.
    /// </summary>
    public static (decimal Calories, decimal Protein, decimal Carbs, decimal Fat) Calculate(
        Food food,
        decimal quantityGrams)
    {
        var factor = quantityGrams / 100m;

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
        CalculateRecipeTotals(IEnumerable<(Food Food, decimal QuantityGrams)> ingredients)
    {
        decimal cal = 0, pro = 0, carb = 0, fat = 0;
        foreach (var (food, qty) in ingredients)
        {
            var (c, p, cb, f) = Calculate(food, qty);
            cal += c; pro += p; carb += cb; fat += f;
        }
        return (Math.Round(cal, 1), Math.Round(pro, 1), Math.Round(carb, 1), Math.Round(fat, 1));
    }
}
