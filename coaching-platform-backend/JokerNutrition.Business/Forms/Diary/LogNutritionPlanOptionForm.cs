using System.ComponentModel.DataAnnotations;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Diary;

public class LogNutritionPlanOptionForm
{
    public int AssignmentId { get; set; }
    public int MealBlockId { get; set; }
    public int MealOptionId { get; set; }
    public DateOnly Date { get; set; }

    [EnumDataType(typeof(MealType))]
    public MealType MealType { get; set; }

    [Range(0.25, 10)]
    public decimal Servings { get; set; } = 1m;

    public List<int> SelectedAlternativeItemIds { get; set; } = new();
}
