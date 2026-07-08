using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Diary;

public class LogFoodForm
{
    public DateOnly Date { get; set; }
    public MealType MealType { get; set; }
    public int? FoodId { get; set; }
    public int? RecipeId { get; set; }
    public decimal QuantityGrams { get; set; }
}
