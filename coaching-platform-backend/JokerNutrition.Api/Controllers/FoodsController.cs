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

    /// <summary>Create a new food entry in the global database.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateFoodForm form)
    {
        var result = await _foodService.CreateFoodAsync(form);
        return Created($"/api/foods/{result.Id}", result);
    }

    /// <summary>Update an existing food entry.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateFoodForm form)
    {
        var result = await _foodService.UpdateFoodAsync(id, form);
        return Ok(result);
    }

    /// <summary>Soft-delete a food entry (no longer appears in athlete food search).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _foodService.DeleteFoodAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Bulk import foods from a CSV file.
    /// CSV columns: Name, Category, CaloriesPer100g, ProteinPer100g, CarbsPer100g, FatPer100g, FiberPer100g
    /// Invalid rows are skipped and reported in the response.
    /// </summary>
    [HttpPost("bulk-import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkImport([FromForm] BulkImportFoodsForm form)
    {
        var result = await _foodService.BulkImportFoodsAsync(form);
        return Ok(result);
    }
}

