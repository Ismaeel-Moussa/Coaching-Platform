using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.DTOs.Supplements;
using JokerNutrition.Business.DTOs.Workouts;

namespace JokerNutrition.Business.DTOs.Athletes;

public class DailyLogHistoryDto
{
    public DateOnly Date { get; set; }
    public TodaysWorkoutDto? Workout { get; set; }
    public DailyDiaryDto? Nutrition { get; set; }
    public List<SupplementDto> Supplements { get; set; } = new();
}
