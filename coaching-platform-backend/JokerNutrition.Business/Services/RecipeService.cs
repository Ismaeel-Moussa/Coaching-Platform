using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Business.DTOs.Recipes;
using JokerNutrition.Business.Forms.Recipes;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IRecipeService
{
    Task<PagedResult<RecipeDto>> GetRecipesAsync(RecipeCategory? category, string? search, int page, int pageSize);
    Task<RecipeDto> GetRecipeByIdAsync(int id);
    Task<RecipeDto> CreateRecipeAsync(CreateRecipeForm form);
    Task<RecipeDto> UpdateRecipeAsync(int recipeId, UpdateRecipeForm form);
    Task<RecipeDto> UploadRecipeImageAsync(int recipeId, IFormFile image);
    Task<DailyDiaryDto> QuickAddToDiaryAsync(int recipeId, MealType mealType);
}

public class RecipeService : _BaseService, IRecipeService
{
    private readonly IRecipeRepository _recipeRepo;
    private readonly IFoodRepository _foodRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly IDiaryService _diaryService;
    private readonly IMealLogRepository _mealLogRepo;
    private readonly IBlobStorageService _blobService;
    private readonly ICacheService _cacheService;

    public RecipeService(
        IPrincipal principal,
        ILogger<RecipeService> logger,
        IRecipeRepository recipeRepo,
        IFoodRepository foodRepo,
        IAthleteRepository athleteRepo,
        IDiaryService diaryService,
        IMealLogRepository mealLogRepo,
        IBlobStorageService blobService,
        ICacheService cacheService)
        : base(principal, logger)
    {
        _recipeRepo = recipeRepo;
        _foodRepo = foodRepo;
        _athleteRepo = athleteRepo;
        _diaryService = diaryService;
        _mealLogRepo = mealLogRepo;
        _blobService = blobService;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<RecipeDto>> GetRecipesAsync(RecipeCategory? category, string? search, int page, int pageSize)
    {
        string cacheKey = $"recipes:search:{category}:{search}:{page}:{pageSize}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var query = _recipeRepo.QueryAll()
                .Include(r => r.Ingredients).ThenInclude(i => i.Food);

            var filtered = category.HasValue
                ? query.Where(r => r.Category == category.Value)
                : query;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                filtered = filtered.Where(r => r.Name.ToLower().Contains(searchLower) 
                    || (r.Description != null && r.Description.ToLower().Contains(searchLower)));
            }

            var totalCount = await filtered.CountAsync();

            var recipes = await filtered
                .OrderByDescending(r => r.IsJokerRecipe)
                .ThenBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<RecipeDto>
            {
                Items = recipes.Select(RecipeMapper.Map),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }, TimeSpan.FromMinutes(3));
    }

    public async Task<RecipeDto> GetRecipeByIdAsync(int id)
    {
        string cacheKey = $"recipe:{id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var recipe = await _recipeRepo.QueryAll()
                .Include(r => r.Ingredients).ThenInclude(i => i.Food)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException($"Recipe {id} not found.");

            return RecipeMapper.Map(recipe);
        }, TimeSpan.FromMinutes(5));
    }

    public async Task<RecipeDto> CreateRecipeAsync(CreateRecipeForm form)
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        // Fetch all foods needed for this recipe
        var foodIds = form.Ingredients.Select(i => i.FoodId).Distinct().ToList();
        var foods = await _foodRepo.Query()
            .Where(f => foodIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id);

        var ingredientTuples = form.Ingredients
            .Select(i => (foods[i.FoodId], i.QuantityGrams));

        var (totalCal, totalPro, totalCarb, totalFat) = MacroCalculatorHelper.CalculateRecipeTotals(ingredientTuples);

        var recipe = new Recipe
        {
            Name = form.Name,
            Description = form.Description,
            Category = form.Category,
            PrepTimeMinutes = form.PrepTimeMinutes,
            CookTimeMinutes = form.CookTimeMinutes,
            Servings = form.Servings,
            IsJokerRecipe = LoggedInUser.Role == "Admin",
            CreatedByAthleteId = athlete?.Id,
            VideoUrl = form.VideoUrl,
            CreatedAt = DateTime.UtcNow,
            TotalCalories = totalCal,
            TotalProtein = totalPro,
            TotalCarbs = totalCarb,
            TotalFat = totalFat,
            Ingredients = form.Ingredients.Select(i => new RecipeIngredient
            {
                FoodId = i.FoodId,
                Food = foods[i.FoodId],
                QuantityGrams = i.QuantityGrams
            }).ToList()
        };

        await _recipeRepo.CreateAsync(recipe);
        await _recipeRepo.SaveChangesAsync();

        _cacheService.EvictByPrefix("recipes:search:");

        return RecipeMapper.Map(recipe);
    }

    public async Task<RecipeDto> UpdateRecipeAsync(int recipeId, UpdateRecipeForm form)
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        var recipe = await _recipeRepo.Query()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} not found.");

        if (LoggedInUser.Role != "Admin")
        {
            if (recipe.CreatedByAthleteId != athlete?.Id)
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this recipe.");
            }
        }

        recipe.Name = form.Name;
        recipe.Description = form.Description;
        recipe.Category = form.Category;
        recipe.PrepTimeMinutes = form.PrepTimeMinutes;
        recipe.CookTimeMinutes = form.CookTimeMinutes;
        recipe.Servings = form.Servings;
        recipe.VideoUrl = form.VideoUrl;

        recipe.Ingredients.Clear();

        var foodIds = form.Ingredients.Select(i => i.FoodId).Distinct().ToList();
        var foods = await _foodRepo.Query()
            .Where(f => foodIds.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id);

        var ingredientTuples = form.Ingredients
            .Select(i => (foods[i.FoodId], i.QuantityGrams));

        var (totalCal, totalPro, totalCarb, totalFat) = MacroCalculatorHelper.CalculateRecipeTotals(ingredientTuples);

        recipe.TotalCalories = totalCal;
        recipe.TotalProtein = totalPro;
        recipe.TotalCarbs = totalCarb;
        recipe.TotalFat = totalFat;

        recipe.Ingredients = form.Ingredients.Select(i => new RecipeIngredient
        {
            RecipeId = recipe.Id,
            FoodId = i.FoodId,
            Food = foods[i.FoodId],
            QuantityGrams = i.QuantityGrams
        }).ToList();

        _recipeRepo.Update(recipe);
        await _recipeRepo.SaveChangesAsync();

        _cacheService.EvictByPrefix("recipes:search:");
        _cacheService.Evict($"recipe:{recipeId}");

        var updatedRecipe = await _recipeRepo.Query()
            .Include(r => r.Ingredients).ThenInclude(i => i.Food)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        return RecipeMapper.Map(updatedRecipe!);
    }

    public async Task<RecipeDto> UploadRecipeImageAsync(int recipeId, IFormFile image)
    {
        var recipe = await _recipeRepo.Query()
            .Include(r => r.Ingredients).ThenInclude(i => i.Food)
            .FirstOrDefaultAsync(r => r.Id == recipeId)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} not found.");

        if (!string.IsNullOrEmpty(recipe.ImageUrl))
        {
            await _blobService.DeleteFileAsync(recipe.ImageUrl);
        }

        var blobName = $"recipes/{recipeId}/image.jpg";
        string imageUrl;
        using (var stream = image.OpenReadStream())
        {
            imageUrl = await _blobService.UploadFileAsync(stream, blobName, image.ContentType);
        }

        recipe.ImageUrl = imageUrl;
        _recipeRepo.Update(recipe);
        await _recipeRepo.SaveChangesAsync();

        _cacheService.EvictByPrefix("recipes:search:");
        _cacheService.Evict($"recipe:{recipeId}");

        return RecipeMapper.Map(recipe);
    }

    public async Task<DailyDiaryDto> QuickAddToDiaryAsync(int recipeId, MealType mealType)
    {
        var userId = LoggedInUser.Id;
        var athlete = await _athleteRepo.Query()
            .FirstOrDefaultAsync(a => a.UserId == userId)
            ?? throw new UnauthorizedAccessException("Athlete profile not found.");

        var recipe = await _recipeRepo.Query()
            .Include(r => r.Ingredients).ThenInclude(i => i.Food)
            .FirstOrDefaultAsync(r => r.Id == recipeId)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var diary = await _diaryService.GetOrCreateDiaryAsync(athlete.Id, today);

        var log = new MealLog
        {
            DailyDiaryId = diary.Id,
            RecipeId = recipe.Id,
            Recipe = recipe,
            MealType = mealType,
            QuantityGrams = recipe.Ingredients.Sum(i => i.QuantityGrams),
            Calories = recipe.TotalCalories,
            Protein = recipe.TotalProtein,
            Carbs = recipe.TotalCarbs,
            Fat = recipe.TotalFat,
            LoggedAt = DateTime.UtcNow
        };

        await _mealLogRepo.CreateAsync(log);
        await _mealLogRepo.SaveChangesAsync();

        // Return refreshed full diary
        var allLogs = await _mealLogRepo.Query()
            .Include(l => l.Food)
            .Include(l => l.Recipe)
            .Where(l => l.DailyDiaryId == diary.Id)
            .OrderBy(l => l.LoggedAt)
            .ToListAsync();

        return DiaryMapper.Map(diary, allLogs);
    }
}
