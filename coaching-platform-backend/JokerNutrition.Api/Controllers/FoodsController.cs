using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Foods;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/foods")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize]
public class FoodsController : ControllerBase
{
    private readonly IFoodService _foodService;

    public FoodsController(IFoodService foodService)
    {
        _foodService = foodService;
    }

    /// <summary>Search foods by name and/or category with pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> SearchFoods([FromQuery] SearchFoodsForm form)
    {
        var result = await _foodService.SearchFoodsAsync(form);
        return Ok(result);
    }

    /// <summary>Get a single food item by ID with full macro-per-100g data.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFoodById(int id)
    {
        var result = await _foodService.GetFoodByIdAsync(id);
        return Ok(result);
    }
}
