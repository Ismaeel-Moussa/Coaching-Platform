using System.Security.Principal;
using System.Text.Json;
using System.Data;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.DTOs.NutritionPlans;
using JokerNutrition.Business.Forms.Diary;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IMealLogService
{
    Task<MealLogDto> LogFoodAsync(LogFoodForm form);
    Task<IReadOnlyList<MealLogDto>> BulkLogFoodsAsync(BulkLogFoodForm form);
    Task<NutritionPlanDiaryEntryDto> LogNutritionPlanOptionAsync(LogNutritionPlanOptionForm form);
    Task<IReadOnlyList<NutritionPlanDiaryEntryDto>> GetNutritionPlanEntriesAsync(int assignmentId, DateOnly date);
    Task<IReadOnlyList<object>> GetFilteredItemsAsync(string type, string source);
    Task<bool> ToggleFavoriteFoodAsync(int foodId);
    Task<bool> ToggleFavoriteRecipeAsync(int recipeId);
    Task RemoveLogEntryAsync(int mealLogId);
    Task RemoveNutritionPlanEntryAsync(int entryId);
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
    private readonly JokerNutritionContext _context;

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
        INotificationService notificationService,
        JokerNutritionContext context)
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
        _context = context;
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

    public async Task<NutritionPlanDiaryEntryDto> LogNutritionPlanOptionAsync(LogNutritionPlanOptionForm form)
    {
        if (form.MealType is not (MealType.Breakfast or MealType.Lunch or MealType.Dinner or MealType.Snack))
            throw new ArgumentException("Plan meals can only be logged as breakfast, lunch, dinner, or snack.");

        var athlete = await GetCurrentAthleteAsync();
        var assignment = await _context.NutritionPlanAssignments.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == form.AssignmentId && item.AthleteId == athlete.Id)
            ?? throw new KeyNotFoundException($"Nutrition plan assignment {form.AssignmentId} not found.");

        var entryDate = form.Date;
        var startDate = DateOnly.FromDateTime(assignment.StartDate);
        var endDate = assignment.EndDate.HasValue ? DateOnly.FromDateTime(assignment.EndDate.Value) : (DateOnly?)null;
        if (!assignment.IsActive || entryDate < startDate || (endDate.HasValue && entryDate >= endDate.Value))
            throw new InvalidOperationException("This nutrition plan was not active on the selected date.");

        var plan = JsonSerializer.Deserialize<NutritionPlanDto>(
            assignment.SnapshotJson, NutritionPlanMapper.SnapshotJsonOptions)
            ?? throw new InvalidOperationException("The assigned nutrition plan snapshot is invalid.");
        var block = plan.MealBlocks.FirstOrDefault(item => item.Id == form.MealBlockId)
            ?? throw new KeyNotFoundException($"Meal block {form.MealBlockId} was not found in this assignment.");
        var option = block.Options.FirstOrDefault(item => item.Id == form.MealOptionId)
            ?? throw new KeyNotFoundException($"Meal option {form.MealOptionId} was not found in this assignment.");
        var items = SelectOptionItems(option, form.SelectedAlternativeItemIds);
        var selectionKey = string.Join(",", form.SelectedAlternativeItemIds.Distinct().OrderBy(id => id));

        var diary = await _diaryService.GetOrCreateDiaryAsync(athlete.Id, form.Date);
        var executionStrategy = _context.Database.CreateExecutionStrategy();
        try
        {
            await executionStrategy.ExecuteAsync(async () =>
            {
                _context.ChangeTracker.Clear();
                await using var transaction = _context.Database.IsRelational()
                    ? await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable)
                    : null;

                var assignmentStillActive = await _context.NutritionPlanAssignments.AsNoTracking()
                    .AnyAsync(item => item.Id == assignment.Id && item.AthleteId == athlete.Id && item.IsActive);
                if (!assignmentStillActive)
                    throw new InvalidOperationException("This nutrition plan is no longer active.");

                var existingEntry = await _context.NutritionPlanDiaryEntries.AsNoTracking()
                    .FirstOrDefaultAsync(entry =>
                    entry.DailyDiaryId == diary.Id &&
                    entry.NutritionPlanAssignmentId == assignment.Id &&
                    entry.NutritionMealBlockId == block.Id);
                if (existingEntry is not null &&
                    (existingEntry.NutritionMealOptionId != option.Id ||
                     existingEntry.MealType != form.MealType ||
                     existingEntry.Servings != form.Servings ||
                     existingEntry.SelectionKey != selectionKey))
                    throw new ConflictException("A meal option from this plan block is already logged for the selected date.");
                if (existingEntry is not null)
                {
                    if (transaction is not null)
                        await transaction.CommitAsync();
                    return;
                }

                if (block.TrainingDayOnly || block.RestDayOnly)
                {
                    var loggedBlockIds = await _context.NutritionPlanDiaryEntries
                        .Where(entry => entry.DailyDiaryId == diary.Id &&
                                        entry.NutritionPlanAssignmentId == assignment.Id)
                        .Select(entry => entry.NutritionMealBlockId)
                        .ToListAsync();
                    var hasOppositeDayEntry = plan.MealBlocks.Any(existingBlock =>
                        loggedBlockIds.Contains(existingBlock.Id) &&
                        ((block.TrainingDayOnly && existingBlock.RestDayOnly) ||
                         (block.RestDayOnly && existingBlock.TrainingDayOnly)));
                    if (hasOppositeDayEntry)
                        throw new ConflictException(
                            "This date already contains meals from the other plan day type. Remove them before switching between training and rest days.");
                }

                var entry = new NutritionPlanDiaryEntry
                {
                    DailyDiaryId = diary.Id,
                    NutritionPlanAssignmentId = assignment.Id,
                    NutritionMealBlockId = block.Id,
                    NutritionMealOptionId = option.Id,
                    SelectionKey = selectionKey,
                    MealType = form.MealType,
                    Servings = form.Servings,
                    LoggedAt = DateTime.UtcNow
                };

                foreach (var item in items)
                {
                    var log = await CreateNutritionPlanMealLogAsync(
                        diary.Id, form.MealType, item, form.Servings);
                    entry.MealLogs.Add(log);
                }

                _context.NutritionPlanDiaryEntries.Add(entry);
                await _context.SaveChangesAsync();
                if (transaction is not null)
                    await transaction.CommitAsync();
            });
        }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();
            var duplicate = await _context.NutritionPlanDiaryEntries.AsNoTracking().FirstOrDefaultAsync(entry =>
                entry.DailyDiaryId == diary.Id &&
                entry.NutritionPlanAssignmentId == assignment.Id &&
                entry.NutritionMealBlockId == block.Id);
            if (duplicate is not null &&
                (duplicate.NutritionMealOptionId != option.Id ||
                 duplicate.MealType != form.MealType ||
                 duplicate.Servings != form.Servings ||
                 duplicate.SelectionKey != selectionKey))
                throw new ConflictException("A meal option from this plan block is already logged for the selected date.");
            if (duplicate is null)
                throw;
        }

        var savedEntry = await _context.NutritionPlanDiaryEntries.AsNoTracking()
            .Include(entry => entry.DailyDiary)
            .Include(entry => entry.MealLogs).ThenInclude(log => log.Food)
            .Include(entry => entry.MealLogs).ThenInclude(log => log.Recipe)
            .SingleAsync(entry =>
                entry.DailyDiaryId == diary.Id &&
                entry.NutritionPlanAssignmentId == assignment.Id &&
                entry.NutritionMealBlockId == block.Id);

        await NotifyCoachAsync(athlete, "PlanMealLogged");
        return DiaryMapper.Map(savedEntry);
    }

    public async Task<IReadOnlyList<NutritionPlanDiaryEntryDto>> GetNutritionPlanEntriesAsync(
        int assignmentId, DateOnly date)
    {
        var athlete = await GetCurrentAthleteAsync();
        var ownsAssignment = await _context.NutritionPlanAssignments.AsNoTracking()
            .AnyAsync(assignment => assignment.Id == assignmentId && assignment.AthleteId == athlete.Id);
        if (!ownsAssignment)
            throw new KeyNotFoundException($"Nutrition plan assignment {assignmentId} not found.");

        var entries = await _context.NutritionPlanDiaryEntries.AsNoTracking()
            .Where(entry => entry.NutritionPlanAssignmentId == assignmentId &&
                            entry.DailyDiary.AthleteId == athlete.Id &&
                            entry.DailyDiary.Date == date)
            .Include(entry => entry.DailyDiary)
            .Include(entry => entry.MealLogs).ThenInclude(log => log.Food)
            .Include(entry => entry.MealLogs).ThenInclude(log => log.Recipe)
            .OrderBy(entry => entry.LoggedAt)
            .ToListAsync();
        return entries.Select(DiaryMapper.Map).ToList();
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

        if (log.NutritionPlanDiaryEntryId.HasValue)
            throw new InvalidOperationException(
                "This item belongs to an assigned-plan meal. Remove the whole plan meal instead.");

        _mealLogRepo.Delete(log);
        await _mealLogRepo.SaveChangesAsync();

        await NotifyCoachAsync(athlete, "MealRemoved");
    }

    public async Task RemoveNutritionPlanEntryAsync(int entryId)
    {
        var athlete = await GetCurrentAthleteAsync();
        var entry = await _context.NutritionPlanDiaryEntries
            .Include(item => item.DailyDiary)
            .FirstOrDefaultAsync(item => item.Id == entryId && item.DailyDiary.AthleteId == athlete.Id)
            ?? throw new KeyNotFoundException($"Nutrition plan diary entry {entryId} not found.");

        _context.NutritionPlanDiaryEntries.Remove(entry);
        await _context.SaveChangesAsync();
        await NotifyCoachAsync(athlete, "PlanMealRemoved");
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
                .Where(f => f.AthleteId == athleteId && f.Recipe.ContentStatus == ContentStatus.Published)
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
        .Where(r => ids.Contains(r.Id) && r.ContentStatus == ContentStatus.Published)
        .Include(r => r.Ingredients).ThenInclude(i => i.Food).ToListAsync();

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
            .FirstOrDefaultAsync(r => r.Id == recipeId!.Value && r.ContentStatus == ContentStatus.Published)
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

    private static IReadOnlyList<NutritionOptionItemDto> SelectOptionItems(
        NutritionMealOptionDto option,
        IReadOnlyCollection<int> selectedAlternativeItemIds)
    {
        var selectedIds = selectedAlternativeItemIds.Distinct().ToHashSet();
        var groupedItems = option.Items
            .Where(item => !string.IsNullOrWhiteSpace(item.AlternativeGroupKey))
            .GroupBy(item => item.AlternativeGroupKey!, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var validAlternativeIds = groupedItems.SelectMany(group => group).Select(item => item.Id).ToHashSet();
        if (selectedIds.Any(id => !validAlternativeIds.Contains(id)))
            throw new ArgumentException("One or more selected alternatives do not belong to this meal option.");

        var result = option.Items.Where(item => string.IsNullOrWhiteSpace(item.AlternativeGroupKey)).ToList();
        foreach (var group in groupedItems)
        {
            var selected = group.Where(item => selectedIds.Contains(item.Id)).ToList();
            if (selected.Count != 1)
                throw new ArgumentException($"Select exactly one item from alternative group '{group.Key}'.");
            result.Add(selected[0]);
        }

        if (result.Count == 0)
            throw new InvalidOperationException("This meal option has no items that can be logged.");
        return result;
    }

    private async Task<decimal> ResolveQuantityGramsAsync(NutritionOptionItemDto item, decimal servings)
    {
        if (!item.FoodId.HasValue && !item.RecipeId.HasValue)
            throw new InvalidOperationException(
                "This option contains a custom text item. Ask your coach to link it to a food or recipe before adding it to the diary.");

        if (item.Unit == IngredientUnit.Gram)
            return item.Quantity * servings;

        if (item.RecipeId.HasValue && item.Unit == IngredientUnit.Piece)
        {
            var totalWeight = item.RecipeTotalWeightGrams ?? await _context.Recipes.AsNoTracking()
                .Where(recipe => recipe.Id == item.RecipeId.Value)
                .Select(recipe => recipe.Ingredients.Sum(ingredient => ingredient.QuantityGrams))
                .FirstOrDefaultAsync();
            if (totalWeight > 0)
                return totalWeight * item.Quantity * servings;
        }

        throw new InvalidOperationException(
            $"'{item.FoodName ?? item.RecipeName ?? item.ItemName}' uses {item.Unit}. Use grams in the plan so its macros can be calculated accurately.");
    }

    private async Task<MealLog> CreateNutritionPlanMealLogAsync(
        int diaryId,
        MealType mealType,
        NutritionOptionItemDto item,
        decimal servings)
    {
        var quantityGrams = await ResolveQuantityGramsAsync(item, servings);
        var snapshotName = item.FoodName ?? item.RecipeName ?? item.ItemName;
        var snapshotNameAr = item.FoodNameAr ?? item.RecipeNameAr ?? item.ItemNameAr;

        if (item.FoodId.HasValue &&
            item.CaloriesPer100Grams.HasValue && item.ProteinPer100Grams.HasValue &&
            item.CarbsPer100Grams.HasValue && item.FatPer100Grams.HasValue)
        {
            var foodStillExists = await _context.Foods.AsNoTracking().AnyAsync(food => food.Id == item.FoodId.Value);
            var factor = quantityGrams / 100m;
            return new MealLog
            {
                DailyDiaryId = diaryId,
                FoodId = foodStillExists ? item.FoodId : null,
                SnapshotName = snapshotName,
                SnapshotNameAr = snapshotNameAr,
                MealType = mealType,
                QuantityGrams = quantityGrams,
                Calories = item.CaloriesPer100Grams.Value * factor,
                Protein = item.ProteinPer100Grams.Value * factor,
                Carbs = item.CarbsPer100Grams.Value * factor,
                Fat = item.FatPer100Grams.Value * factor,
                LoggedAt = DateTime.UtcNow
            };
        }

        if (item.RecipeId.HasValue && item.RecipeTotalWeightGrams > 0 &&
            item.RecipeTotalCalories.HasValue && item.RecipeTotalProtein.HasValue &&
            item.RecipeTotalCarbs.HasValue && item.RecipeTotalFat.HasValue)
        {
            var recipeStillExists = await _context.Recipes.AsNoTracking().AnyAsync(recipe => recipe.Id == item.RecipeId.Value);
            var factor = quantityGrams / item.RecipeTotalWeightGrams.Value;
            return new MealLog
            {
                DailyDiaryId = diaryId,
                RecipeId = recipeStillExists ? item.RecipeId : null,
                SnapshotName = snapshotName,
                SnapshotNameAr = snapshotNameAr,
                MealType = mealType,
                QuantityGrams = quantityGrams,
                Calories = item.RecipeTotalCalories.Value * factor,
                Protein = item.RecipeTotalProtein.Value * factor,
                Carbs = item.RecipeTotalCarbs.Value * factor,
                Fat = item.RecipeTotalFat.Value * factor,
                LoggedAt = DateTime.UtcNow
            };
        }

        // Backward compatibility for assignments created before nutrient values
        // were included in their snapshots.
        var legacyLog = await CreateMealLogAsync(
            diaryId, mealType, item.FoodId, item.RecipeId, quantityGrams);
        legacyLog.SnapshotName = snapshotName;
        legacyLog.SnapshotNameAr = snapshotNameAr;
        return legacyLog;
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
