using System.Security.Principal;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.Forms.Diary;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IMealLogService
{
    Task<MealLogDto> LogFoodAsync(LogFoodForm form);
    Task RemoveLogEntryAsync(int mealLogId);
}

public class MealLogService : _BaseService, IMealLogService
{
    private readonly IMealLogRepository _mealLogRepo;
    private readonly IFoodRepository _foodRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly IDiaryService _diaryService;

    public MealLogService(
        IPrincipal principal,
        ILogger<MealLogService> logger,
        IMealLogRepository mealLogRepo,
        IFoodRepository foodRepo,
        IRecipeRepository recipeRepo,
        IAthleteRepository athleteRepo,
        IDiaryService diaryService)
        : base(principal, logger)
    {
        _mealLogRepo = mealLogRepo;
        _foodRepo = foodRepo;
        _recipeRepo = recipeRepo;
        _athleteRepo = athleteRepo;
        _diaryService = diaryService;
    }

    public async Task<MealLogDto> LogFoodAsync(LogFoodForm form)
    {
        if (form.FoodId is null && form.RecipeId is null)
            throw new ArgumentException("Either FoodId or RecipeId must be provided.");

        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");

        var diary = await _diaryService.GetOrCreateDiaryAsync(athlete.Id, form.Date);

        MealLog log;

        if (form.FoodId.HasValue)
        {
            var food = await _foodRepo.GetByIdAsync(form.FoodId.Value)
                ?? throw new KeyNotFoundException($"Food {form.FoodId} not found.");

            var (cal, pro, carb, fat) = MacroCalculatorHelper.Calculate(food, form.QuantityGrams, form.State);

            log = new MealLog
            {
                DailyDiaryId = diary.Id,
                FoodId = food.Id,
                Food = food,
                MealType = form.MealType,
                QuantityGrams = form.QuantityGrams,
                State = form.State,
                Calories = cal,
                Protein = pro,
                Carbs = carb,
                Fat = fat,
                LoggedAt = DateTime.UtcNow
            };
        }
        else
        {
            var recipe = await _recipeRepo.Query()
                .Include(r => r.Ingredients).ThenInclude(i => i.Food)
                .FirstOrDefaultAsync(r => r.Id == form.RecipeId!.Value)
                ?? throw new KeyNotFoundException($"Recipe {form.RecipeId} not found.");

            // For a recipe log, quantity is servings × total; here quantity is total grams of all ingredients
            log = new MealLog
            {
                DailyDiaryId = diary.Id,
                RecipeId = recipe.Id,
                Recipe = recipe,
                MealType = form.MealType,
                QuantityGrams = form.QuantityGrams,
                State = form.State,
                Calories = recipe.TotalCalories,
                Protein = recipe.TotalProtein,
                Carbs = recipe.TotalCarbs,
                Fat = recipe.TotalFat,
                LoggedAt = DateTime.UtcNow
            };
        }

        await _mealLogRepo.CreateAsync(log);
        await _mealLogRepo.SaveChangesAsync();

        return DiaryMapper.Map(log);
    }

    public async Task RemoveLogEntryAsync(int mealLogId)
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");

        var log = await _mealLogRepo.Query()
            .Include(l => l.DailyDiary)
            .FirstOrDefaultAsync(l => l.Id == mealLogId && l.DailyDiary.AthleteId == athlete.Id)
            ?? throw new KeyNotFoundException($"Meal log entry {mealLogId} not found.");

        _mealLogRepo.Delete(log);
        await _mealLogRepo.SaveChangesAsync();
    }
}
