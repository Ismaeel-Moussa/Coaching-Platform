using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class DailyDiary
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public DateOnly Date { get; set; }

    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }

    public decimal WaterLitersConsumed { get; set; } = 0;
    public decimal WaterLitersTarget { get; set; } = 4.0m;
    public int StepsWalked { get; set; } = 0;
    public int StepsTarget { get; set; } = 7000;

    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
    public ICollection<NutritionPlanDiaryEntry> NutritionPlanEntries { get; set; } = new List<NutritionPlanDiaryEntry>();
}

public class NutritionPlanDiaryEntry
{
    public int Id { get; set; }
    public int DailyDiaryId { get; set; }
    public DailyDiary DailyDiary { get; set; } = null!;
    public int NutritionPlanAssignmentId { get; set; }
    public NutritionPlanAssignment NutritionPlanAssignment { get; set; } = null!;
    public int NutritionMealBlockId { get; set; }
    public int NutritionMealOptionId { get; set; }
    public string SelectionKey { get; set; } = string.Empty;
    public MealType MealType { get; set; }
    public decimal Servings { get; set; } = 1m;
    public DateTime LoggedAt { get; set; }

    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
}

public class MealLog
{
    public int Id { get; set; }
    public int DailyDiaryId { get; set; }
    public DailyDiary DailyDiary { get; set; } = null!;
    public int? FoodId { get; set; }
    public Food? Food { get; set; }
    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public string? SnapshotName { get; set; }
    public string? SnapshotNameAr { get; set; }
    public int? NutritionPlanDiaryEntryId { get; set; }
    public NutritionPlanDiaryEntry? NutritionPlanDiaryEntry { get; set; }
    public MealType MealType { get; set; }
    public decimal QuantityGrams { get; set; }

    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public DateTime LoggedAt { get; set; }
}
