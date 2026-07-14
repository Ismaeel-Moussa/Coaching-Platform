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
    Task<IReadOnlyList<MealLogDto>> BulkLogFoodsAsync(BulkLogFoodForm form);
    Task<IReadOnlyList<object>> GetFilteredItemsAsync(string type, string source);
    Task<bool> ToggleFavoriteFoodAsync(int foodId);
    Task<bool> ToggleFavoriteRecipeAsync(int recipeId);
    Task RemoveLogEntryAsync(int mealLogId);
}

public class MealLogService : _BaseService, IMealLogService
{
    private readonly IMealLogRepository _mealLogRepo;
    private readonly IFoodRepository _foodRepo;
    private readonly IRecipeRepository _recipeRepo;
    private readonly IFavoriteFoodRepository _favoriteFoodRepo;
    private readonly IFavoriteRecipeRepository _favoriteRecipeRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly IDiaryService _diaryService;
    private readonly INotificationService _notificationService;

    public MealLogService(
        IPrincipal principal,
        ILogger<MealLogService> logger,
        IMealLogRepository mealLogRepo,
        IFoodRepository foodRepo,
        IRecipeRepository recipeRepo,
        IFavoriteFoodRepository favoriteFoodRepo,
        IFavoriteRecipeRepository favoriteRecipeRepo,
        IAthleteRepository athleteRepo,
        IDiaryService diaryService,
        INotificationService notificationService)
        : base(principal, logger)
    {
        _mealLogRepo = mealLogRepo;
        _foodRepo = foodRepo;
        _recipeRepo = recipeRepo;
        _favoriteFoodRepo = favoriteFoodRepo;
        _favoriteRecipeRepo = favoriteRecipeRepo;
        _athleteRepo = athleteRepo;
        _diaryService = diaryService;
        _notificationService = notificationService;
    }

    public async Task<MealLogDto> LogFoodAsync(LogFoodForm form)
    {
        ValidateItem(form.FoodId, form.RecipeId, form.QuantityGrams);

        var athlete = await GetCurrentAthleteAsync();

        var diary = await _diaryService.GetOrCreateDiaryAsync(athlete.Id, form.Date);
        var log = await CreateMealLogAsync(diary.Id, form.MealType, form.FoodId, form.RecipeId, form.QuantityGrams);

        await _mealLogRepo.CreateAsync(log);
        await _mealLogRepo.SaveChangesAsync();
        await NotifyCoachAsync(athlete, "MealLogged");

        return DiaryMapper.Map(log);
    }

    public async Task<IReadOnlyList<MealLogDto>> BulkLogFoodsAsync(BulkLogFoodForm form)
    {
        if (form.Items == null || form.Items.Count == 0)
            throw new ArgumentException("At least one item must be provided.");
        if (form.Items.Count > 50)
            throw new ArgumentException("A maximum of 50 items can be logged at once.");

        foreach (var item in form.Items)
            ValidateItem(item.FoodId, item.RecipeId, item.QuantityGrams);

        var athlete = await GetCurrentAthleteAsync();
        var diary = await _diaryService.GetOrCreateDiaryAsync(athlete.Id, form.Date);
        var logs = new List<MealLog>();

        foreach (var item in form.Items)
        {
            logs.Add(await CreateMealLogAsync(
                diary.Id, form.MealType, item.FoodId, item.RecipeId, item.QuantityGrams));
        }

        await _mealLogRepo.CreateRangeAsync(logs);
        await _mealLogRepo.SaveChangesAsync();
        await NotifyCoachAsync(athlete, "MealsLogged");

        return logs.Select(DiaryMapper.Map).ToList();
    }

    public async Task<IReadOnlyList<object>> GetFilteredItemsAsync(string type, string source)
    {
        var athlete = await GetCurrentAthleteAsync();
        var normalizedType = type.Trim().ToLowerInvariant();
        var normalizedSource = source.Trim().ToLowerInvariant();

        if (normalizedType is not ("food" or "recipe"))
            throw new ArgumentException("Type must be either 'food' or 'recipe'.");
        if (normalizedSource is not ("recent" or "frequent" or "favorites"))
            throw new ArgumentException("Source must be 'recent', 'frequent', or 'favorites'.");

        return normalizedType == "food"
            ? await GetFilteredFoodsAsync(athlete.Id, normalizedSource)
            : await GetFilteredRecipesAsync(athlete.Id, normalizedSource);
    }

    public async Task<bool> ToggleFavoriteFoodAsync(int foodId)
    {
        var athlete = await GetCurrentAthleteAsync();
        var favorite = await _favoriteFoodRepo.Query()
            .FirstOrDefaultAsync(f => f.AthleteId == athlete.Id && f.FoodId == foodId);

        if (favorite is not null)
        {
            _favoriteFoodRepo.Delete(favorite);
            await _favoriteFoodRepo.SaveChangesAsync();
            return false;
        }

        if (!await _foodRepo.QueryAll().AnyAsync(f => f.Id == foodId))
            throw new KeyNotFoundException($"Food {foodId} not found.");

        await _favoriteFoodRepo.CreateAsync(new FavoriteFood
        {
            AthleteId = athlete.Id,
            FoodId = foodId,
            CreatedAt = DateTime.UtcNow
        });
        await _favoriteFoodRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleFavoriteRecipeAsync(int recipeId)
    {
        var athlete = await GetCurrentAthleteAsync();
        var favorite = await _favoriteRecipeRepo.Query()
            .FirstOrDefaultAsync(f => f.AthleteId == athlete.Id && f.RecipeId == recipeId);

        if (favorite is not null)
        {
            _favoriteRecipeRepo.Delete(favorite);
            await _favoriteRecipeRepo.SaveChangesAsync();
            return false;
        }

        if (!await _recipeRepo.QueryAll().AnyAsync(r => r.Id == recipeId))
            throw new KeyNotFoundException($"Recipe {recipeId} not found.");

        await _favoriteRecipeRepo.CreateAsync(new FavoriteRecipe
        {
            AthleteId = athlete.Id,
            RecipeId = recipeId,
            CreatedAt = DateTime.UtcNow
        });
        await _favoriteRecipeRepo.SaveChangesAsync();
        return true;
    }

    public async Task RemoveLogEntryAsync(int mealLogId)
    {
        var athlete = await GetCurrentAthleteAsync();

        var log = await _mealLogRepo.Query()
            .Include(l => l.DailyDiary)
            .FirstOrDefaultAsync(l => l.Id == mealLogId && l.DailyDiary.AthleteId == athlete.Id)
            ?? throw new KeyNotFoundException($"Meal log entry {mealLogId} not found.");

        _mealLogRepo.Delete(log);
        await _mealLogRepo.SaveChangesAsync();

        await NotifyCoachAsync(athlete, "MealRemoved");
    }

    private async Task<IReadOnlyList<object>> GetFilteredFoodsAsync(int athleteId, string source)
    {
        var favoriteIds = (await _favoriteFoodRepo.QueryAll()
            .Where(f => f.AthleteId == athleteId).Select(f => f.FoodId).ToListAsync()).ToHashSet();
        List<Food> foods;

        if (source == "favorites")
        {
            foods = await _favoriteFoodRepo.QueryAll()
                .Where(f => f.AthleteId == athleteId).OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Food).ToListAsync();
        }
        else if (source == "recent")
        {
            var entries = await _mealLogRepo.QueryAll()
                .Where(l => l.DailyDiary.AthleteId == athleteId && l.FoodId.HasValue)
                .OrderByDescending(l => l.LoggedAt).Select(l => new { l.FoodId, l.LoggedAt })
                .ToListAsync();
            var ids = entries.GroupBy(x => x.FoodId!.Value).Take(30).Select(g => g.Key).ToList();
            foods = await _foodRepo.QueryAll().Where(f => ids.Contains(f.Id)).ToListAsync();
            var foodMap = foods.ToDictionary(f => f.Id);
            foods = ids.Select(id => foodMap.GetValueOrDefault(id)).Where(f => f != null).ToList()!;
        }
        else
        {
            var ids = await _mealLogRepo.QueryAll()
                .Where(l => l.DailyDiary.AthleteId == athleteId && l.FoodId.HasValue)
                .GroupBy(l => l.FoodId!.Value).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Max(l => l.LoggedAt))
                .Select(g => g.Key).Take(30).ToListAsync();
            foods = await _foodRepo.QueryAll().Where(f => ids.Contains(f.Id)).ToListAsync();
            var foodMap = foods.ToDictionary(f => f.Id);
            foods = ids.Select(id => foodMap.GetValueOrDefault(id)).Where(f => f != null).ToList()!;
        }

        return foods.Select(f => (object)FoodMapper.Map(f, favoriteIds.Contains(f.Id))).ToList();
    }

    private async Task<IReadOnlyList<object>> GetFilteredRecipesAsync(int athleteId, string source)
    {
        var favoriteIds = (await _favoriteRecipeRepo.QueryAll()
            .Where(f => f.AthleteId == athleteId).Select(f => f.RecipeId).ToListAsync()).ToHashSet();
        List<Recipe> recipes;

        if (source == "favorites")
        {
            recipes = await _favoriteRecipeRepo.QueryAll()
                .Where(f => f.AthleteId == athleteId)
                .Include(f => f.Recipe).ThenInclude(r => r.Ingredients).ThenInclude(i => i.Food)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => f.Recipe).ToListAsync();
        }
        else if (source == "recent")
        {
            var entries = await _mealLogRepo.QueryAll()
                .Where(l => l.DailyDiary.AthleteId == athleteId && l.RecipeId.HasValue)
                .OrderByDescending(l => l.LoggedAt).Select(l => new { l.RecipeId, l.LoggedAt })
                .ToListAsync();
            var ids = entries.GroupBy(x => x.RecipeId!.Value).Take(30).Select(g => g.Key).ToList();
            recipes = await GetRecipesByIdsAsync(ids);
            var recipeMap = recipes.ToDictionary(r => r.Id);
            recipes = ids.Select(id => recipeMap.GetValueOrDefault(id)).Where(r => r != null).ToList()!;
        }
        else
        {
            var ids = await _mealLogRepo.QueryAll()
                .Where(l => l.DailyDiary.AthleteId == athleteId && l.RecipeId.HasValue)
                .GroupBy(l => l.RecipeId!.Value).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Max(l => l.LoggedAt))
                .Select(g => g.Key).Take(30).ToListAsync();
            recipes = await GetRecipesByIdsAsync(ids);
            var recipeMap = recipes.ToDictionary(r => r.Id);
            recipes = ids.Select(id => recipeMap.GetValueOrDefault(id)).Where(r => r != null).ToList()!;
        }

        return recipes.Select(r => (object)RecipeMapper.Map(r, favoriteIds.Contains(r.Id))).ToList();
    }

    private Task<List<Recipe>> GetRecipesByIdsAsync(List<int> ids) => _recipeRepo.QueryAll()
        .Where(r => ids.Contains(r.Id)).Include(r => r.Ingredients).ThenInclude(i => i.Food).ToListAsync();

    private async Task<MealLog> CreateMealLogAsync(int diaryId, JokerNutrition.Data.Enums.MealType mealType, int? foodId, int? recipeId, decimal quantityGrams)
    {
        if (foodId.HasValue)
        {
            var food = await _foodRepo.GetByIdAsync(foodId.Value)
                ?? throw new KeyNotFoundException($"Food {foodId} not found.");
            var (calories, protein, carbs, fat) = MacroCalculatorHelper.Calculate(food, quantityGrams);
            return new MealLog
            {
                DailyDiaryId = diaryId, FoodId = food.Id, Food = food, MealType = mealType,
                QuantityGrams = quantityGrams, Calories = calories, Protein = protein, Carbs = carbs, Fat = fat,
                LoggedAt = DateTime.UtcNow
            };
        }

        var recipe = await _recipeRepo.Query().Include(r => r.Ingredients).ThenInclude(i => i.Food)
            .FirstOrDefaultAsync(r => r.Id == recipeId!.Value)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} not found.");
        var totalWeight = recipe.Ingredients.Sum(i => i.QuantityGrams);
        var multiplier = totalWeight > 0 ? quantityGrams / totalWeight : 1m;
        return new MealLog
        {
            DailyDiaryId = diaryId, RecipeId = recipe.Id, Recipe = recipe, MealType = mealType,
            QuantityGrams = quantityGrams, Calories = recipe.TotalCalories * multiplier,
            Protein = recipe.TotalProtein * multiplier, Carbs = recipe.TotalCarbs * multiplier, Fat = recipe.TotalFat * multiplier,
            LoggedAt = DateTime.UtcNow
        };
    }

    private static void ValidateItem(int? foodId, int? recipeId, decimal quantityGrams)
    {
        if (foodId.HasValue == recipeId.HasValue)
            throw new ArgumentException("Provide exactly one of FoodId or RecipeId.");
        if (quantityGrams <= 0)
            throw new ArgumentException("QuantityGrams must be greater than zero.");
    }

    private async Task<Athlete> GetCurrentAthleteAsync()
    {
        return await _athleteRepo.Query().FirstOrDefaultAsync(a => a.UserId == LoggedInUser.Id)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");
    }

    private async Task NotifyCoachAsync(Athlete athlete, string activityType)
    {
        if (!athlete.AssignedCoachId.HasValue) return;
        var coachUserId = await _athleteRepo.Query().Where(a => a.Id == athlete.Id)
            .Select(a => a.AssignedCoach!.UserId).FirstOrDefaultAsync();
        if (coachUserId > 0)
            await _notificationService.SendDirectUpdateAsync(coachUserId, "AthleteActivity", new { type = activityType, athleteId = athlete.Id });
    }
}
