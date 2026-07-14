using System;
using System.Collections.Generic;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Diary;

public class BulkLogFoodForm
{
    public DateOnly Date { get; set; }
    public MealType MealType { get; set; }
    public List<BulkLogFoodItem> Items { get; set; } = new();
}

public class BulkLogFoodItem
{
    public int? FoodId { get; set; }
    public int? RecipeId { get; set; }
    public decimal QuantityGrams { get; set; }
}
