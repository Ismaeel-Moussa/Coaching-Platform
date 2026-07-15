using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Mappers;

public static class DiaryMapper
{
    public static NutritionPlanDiaryEntryDto Map(NutritionPlanDiaryEntry entry) => new()
    {
        Id = entry.Id,
        AssignmentId = entry.NutritionPlanAssignmentId,
        MealBlockId = entry.NutritionMealBlockId,
        MealOptionId = entry.NutritionMealOptionId,
        Date = entry.DailyDiary.Date,
        MealType = entry.MealType,
        Servings = entry.Servings,
        LoggedAt = entry.LoggedAt,
        MealLogs = entry.MealLogs.OrderBy(log => log.Id).Select(Map).ToList()
    };

    public static MealLogDto Map(MealLog log) => new()
    {
        Id = log.Id,
        MealType = log.MealType,
        Food = log.Food is null ? null : new FoodSummaryDto
        {
            Id = log.Food.Id,
            Name = log.Food.Name,
            Category = log.Food.Category
        },
        Recipe = log.Recipe is null ? null : new RecipeSummaryDto
        {
            Id = log.Recipe.Id,
            Name = log.Recipe.Name
        },
        Name = log.SnapshotName ?? log.Food?.Name ?? log.Recipe?.Name,
        NameAr = log.SnapshotNameAr ?? log.Food?.NameAr ?? log.Recipe?.NameAr,
        NutritionPlanDiaryEntryId = log.NutritionPlanDiaryEntryId,
        QuantityGrams = log.QuantityGrams,
        Calories = log.Calories,
        Protein = log.Protein,
        Carbs = log.Carbs,
        Fat = log.Fat,
        LoggedAt = log.LoggedAt
    };

    public static DailyDiaryDto Map(DailyDiary diary, IEnumerable<MealLog> logs)
    {
        var logList = logs.ToList();

        decimal totalCal = logList.Sum(l => l.Calories);
        decimal totalPro = logList.Sum(l => l.Protein);
        decimal totalCarb = logList.Sum(l => l.Carbs);
        decimal totalFat = logList.Sum(l => l.Fat);

        return new DailyDiaryDto
        {
            Id = diary.Id,
            Date = diary.Date,
            TargetCalories = diary.TargetCalories,
            TargetProtein = diary.TargetProtein,
            TargetCarbs = diary.TargetCarbs,
            TargetFat = diary.TargetFat,
            WaterLitersConsumed = diary.WaterLitersConsumed,
            WaterLitersTarget = diary.WaterLitersTarget,
            StepsWalked = diary.StepsWalked,
            StepsTarget = diary.StepsTarget,
            TotalCalories = totalCal,
            TotalProtein = totalPro,
            TotalCarbs = totalCarb,
            TotalFat = totalFat,
            Breakfast = logList.Where(l => l.MealType == MealType.Breakfast).Select(Map).ToList(),
            Lunch = logList.Where(l => l.MealType == MealType.Lunch).Select(Map).ToList(),
            Dinner = logList.Where(l => l.MealType == MealType.Dinner).Select(Map).ToList(),
            Snack = logList.Where(l => l.MealType == MealType.Snack).Select(Map).ToList(),
            Suhoor = logList.Where(l => l.MealType == MealType.Suhoor).Select(Map).ToList(),
            Iftar = logList.Where(l => l.MealType == MealType.Iftar).Select(Map).ToList(),
            PreWorkout = logList.Where(l => l.MealType == MealType.PreWorkout).Select(Map).ToList(),
            PostWorkout = logList.Where(l => l.MealType == MealType.PostWorkout).Select(Map).ToList()
        };
    }

    public static MacroSummaryDto MapSummary(DailyDiary diary, IEnumerable<MealLog> logs)
    {
        var logList = logs.ToList();

        return new MacroSummaryDto
        {
            Date = diary.Date,
            CaloriesConsumed = logList.Sum(l => l.Calories),
            ProteinConsumed = logList.Sum(l => l.Protein),
            CarbsConsumed = logList.Sum(l => l.Carbs),
            FatConsumed = logList.Sum(l => l.Fat),
            TargetCalories = diary.TargetCalories,
            TargetProtein = diary.TargetProtein,
            TargetCarbs = diary.TargetCarbs,
            TargetFat = diary.TargetFat,
            WaterLitersConsumed = diary.WaterLitersConsumed,
            WaterLitersTarget = diary.WaterLitersTarget,
            StepsWalked = diary.StepsWalked,
            StepsTarget = diary.StepsTarget
        };
    }
}
