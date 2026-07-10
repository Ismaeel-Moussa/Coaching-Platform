using System.Globalization;
using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Foods;
using JokerNutrition.Business.Forms.Foods;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IFoodService
{
    Task<PagedResult<FoodDto>> SearchFoodsAsync(SearchFoodsForm form);
    Task<FoodDto> GetFoodByIdAsync(int id);
    Task<FoodDto> CreateFoodAsync(CreateFoodForm form);
    Task<FoodDto> UpdateFoodAsync(int id, CreateFoodForm form);
    Task DeleteFoodAsync(int id);
    Task<BulkImportResultDto> BulkImportFoodsAsync(BulkImportFoodsForm form);
}

public class FoodService : _BaseService, IFoodService
{
    private readonly IFoodRepository _foodRepo;
    private readonly ICacheService _cacheService;

    public FoodService(
        IPrincipal principal,
        ILogger<FoodService> logger,
        IFoodRepository foodRepo,
        ICacheService cacheService)
        : base(principal, logger)
    {
        _foodRepo = foodRepo;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<FoodDto>> SearchFoodsAsync(SearchFoodsForm form)
    {
        string cacheKey = $"foods:search:{form.Search}:{form.Category}:{form.Page}:{form.PageSize}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var query = _foodRepo.QueryAll();

            if (!string.IsNullOrWhiteSpace(form.Search))
                query = query.Where(f => f.Name.Contains(form.Search));

            if (!string.IsNullOrWhiteSpace(form.Category))
                query = query.Where(f => f.Category == form.Category);

            var totalCount = await query.CountAsync();

            var foods = await query
                .OrderBy(f => f.Name)
                .Skip((form.Page - 1) * form.PageSize)
                .Take(form.PageSize)
                .ToListAsync();

            return new PagedResult<FoodDto>
            {
                Items = foods.Select(FoodMapper.Map),
                TotalCount = totalCount,
                Page = form.Page,
                PageSize = form.PageSize
            };
        }, TimeSpan.FromMinutes(5));
    }

    public async Task<FoodDto> GetFoodByIdAsync(int id)
    {
        string cacheKey = $"food:{id}";
        return await _cacheService.GetOrCreateAsync(cacheKey, async () =>
        {
            var food = await _foodRepo.QueryAll().FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new KeyNotFoundException($"Food with id {id} not found.");
            return FoodMapper.Map(food);
        }, TimeSpan.FromMinutes(10));
    }

    // ─── Admin Mutations ──────────────────────────────────────────────

    public async Task<FoodDto> CreateFoodAsync(CreateFoodForm form)
    {
        var food = new Food
        {
            Name = form.Name,
            Category = form.Category,
            CaloriesPer100g = form.CaloriesPer100g,
            ProteinPer100g = form.ProteinPer100g,
            CarbsPer100g = form.CarbsPer100g,
            FatPer100g = form.FatPer100g,
            FiberPer100g = form.FiberPer100g,
            IsCustom = false
        };

        await _foodRepo.CreateAsync(food);
        await _foodRepo.SaveChangesAsync();

        _cacheService.EvictByPrefix("foods:search:");

        return FoodMapper.Map(food);
    }

    public async Task<FoodDto> UpdateFoodAsync(int id, CreateFoodForm form)
    {
        var food = await _foodRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Food with id {id} not found.");

        food.Name = form.Name;
        food.Category = form.Category;
        food.CaloriesPer100g = form.CaloriesPer100g;
        food.ProteinPer100g = form.ProteinPer100g;
        food.CarbsPer100g = form.CarbsPer100g;
        food.FatPer100g = form.FatPer100g;
        food.FiberPer100g = form.FiberPer100g;

        _foodRepo.Update(food);
        await _foodRepo.SaveChangesAsync();

        _cacheService.EvictByPrefix("foods:search:");
        _cacheService.Evict($"food:{id}");

        return FoodMapper.Map(food);
    }

    public async Task DeleteFoodAsync(int id)
    {
        var food = await _foodRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Food with id {id} not found.");

        _foodRepo.Delete(food);
        await _foodRepo.SaveChangesAsync();

        _cacheService.EvictByPrefix("foods:search:");
        _cacheService.Evict($"food:{id}");
    }

    public async Task<BulkImportResultDto> BulkImportFoodsAsync(BulkImportFoodsForm form)
    {
        var result = new BulkImportResultDto();
        var validFoods = new List<Food>();

        using var reader = new StreamReader(form.CsvFile.OpenReadStream());
        var allLines = await reader.ReadToEndAsync();
        var lines = allLines.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

        // Skip header line
        if (lines.Count == 0) return result;
        lines.RemoveAt(0);

        int rowIndex = 1;
        foreach (var rawLine in lines)
        {
            rowIndex++;
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 7)
            {
                result.Errors.Add($"Row {rowIndex}: Expected 7 columns, got {cols.Length} — skipped.");
                result.SkippedCount++;
                continue;
            }

            var name = cols[0].Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                result.Errors.Add($"Row {rowIndex}: Name is required — skipped.");
                result.SkippedCount++;
                continue;
            }

            if (!decimal.TryParse(cols[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cal) ||
                !decimal.TryParse(cols[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var protein) ||
                !decimal.TryParse(cols[4].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var carbs) ||
                !decimal.TryParse(cols[5].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var fat) ||
                !decimal.TryParse(cols[6].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var fiber))
            {
                result.Errors.Add($"Row {rowIndex} ({name}): Could not parse macro values — skipped.");
                result.SkippedCount++;
                continue;
            }

            // Validate: macro-derived calories must not exceed 900 kcal/100g
            var derivedKcal = (protein * 4) + (carbs * 4) + (fat * 9);
            if (derivedKcal > 900)
            {
                result.Errors.Add($"Row {rowIndex} ({name}): Macro totals exceed 900 kcal/100g — rejected.");
                result.SkippedCount++;
                continue;
            }

            validFoods.Add(new Food
            {
                Name = name,
                Category = cols[1].Trim().Length > 0 ? cols[1].Trim() : null,
                CaloriesPer100g = cal,
                ProteinPer100g = protein,
                CarbsPer100g = carbs,
                FatPer100g = fat,
                FiberPer100g = fiber,
                IsCustom = false
            });
        }

        if (validFoods.Count > 0)
        {
            await _foodRepo.CreateRangeAsync(validFoods);
            await _foodRepo.SaveChangesAsync();
            result.InsertedCount = validFoods.Count;
            _cacheService.EvictByPrefix("foods:search:");
        }

        return result;
    }
}

