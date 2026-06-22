using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Diary;

public class DailyDiaryDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    // Targets snapshot
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }

    // Hydration & steps
    public decimal WaterLitersConsumed { get; set; }
    public decimal WaterLitersTarget { get; set; }
    public int StepsWalked { get; set; }
    public int StepsTarget { get; set; }

    // Meals grouped by type
    public List<MealLogDto> Breakfast { get; set; } = new();
    public List<MealLogDto> Lunch { get; set; } = new();
    public List<MealLogDto> Dinner { get; set; } = new();
    public List<MealLogDto> Snack { get; set; } = new();

    // Ramadan meal types
    public List<MealLogDto> Suhoor { get; set; } = new();
    public List<MealLogDto> Iftar { get; set; } = new();
    public List<MealLogDto> PreWorkout { get; set; } = new();
    public List<MealLogDto> PostWorkout { get; set; } = new();

    // Daily totals
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }
}
