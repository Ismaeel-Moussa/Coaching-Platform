using System.Security.Principal;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Foods;
using JokerNutrition.Business.Forms.Foods;
using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IFoodService
{
    Task<PagedResult<FoodDto>> SearchFoodsAsync(SearchFoodsForm form);
    Task<FoodDto> GetFoodByIdAsync(int id);
}

public class FoodService : _BaseService, IFoodService
{
    private readonly IFoodRepository _foodRepo;

    public FoodService(
        IPrincipal principal,
        ILogger<FoodService> logger,
        IFoodRepository foodRepo)
        : base(principal, logger)
    {
        _foodRepo = foodRepo;
    }

    public async Task<PagedResult<FoodDto>> SearchFoodsAsync(SearchFoodsForm form)
    {
        var query = _foodRepo.Query();

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
    }

    public async Task<FoodDto> GetFoodByIdAsync(int id)
    {
        var food = await _foodRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Food with id {id} not found.");
        return FoodMapper.Map(food);
    }
}
