using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Diary;

public class NutritionPlanDiaryEntryDto
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public int MealBlockId { get; set; }
    public int MealOptionId { get; set; }
    public DateOnly Date { get; set; }
    public MealType MealType { get; set; }
    public decimal Servings { get; set; }
    public DateTime LoggedAt { get; set; }
    public List<MealLogDto> MealLogs { get; set; } = new();
}
