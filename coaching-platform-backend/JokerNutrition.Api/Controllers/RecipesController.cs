using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Recipes;
using JokerNutrition.Business.Services;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/recipes")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    /// <summary>List recipes with optional category filter (MuscleBuilding=0, FatLoss=1, Custom=2).</summary>
    [HttpGet]
    public async Task<IActionResult> GetRecipes(
        [FromQuery] RecipeCategory? category,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _recipeService.GetRecipesAsync(category, search, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a single recipe with full ingredient breakdown and per-ingredient macros.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRecipeById(int id)
    {
        var result = await _recipeService.GetRecipeByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a custom recipe with a list of ingredients. Macros are auto-calculated.</summary>
    [HttpPost]
    [Authorize(Roles = "Athlete,Admin")]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeForm form)
    {
        var result = await _recipeService.CreateRecipeAsync(form);
        return Created("", result);
    }

    /// <summary>Update an existing recipe metadata and ingredients.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Athlete,Admin")]
    public async Task<IActionResult> UpdateRecipe(int id, [FromBody] UpdateRecipeForm form)
    {
        var result = await _recipeService.UpdateRecipeAsync(id, form);
        return Ok(result);
    }

    /// <summary>Upload a recipe image.</summary>
    [HttpPost("{id:int}/image")]
    [Authorize(Roles = "Athlete,Admin")]
    public async Task<IActionResult> UploadRecipeImage(int id, IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No image file provided.");

        var result = await _recipeService.UploadRecipeImageAsync(id, image);
        return Ok(result);
    }

    /// <summary>Quick-add a full recipe to today's diary under a specified meal type.</summary>
    [HttpPost("{id:int}/add-to-diary")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> QuickAddToDiary(int id, [FromQuery] MealType mealType = MealType.Lunch)
    {
        var result = await _recipeService.QuickAddToDiaryAsync(id, mealType);
        return Ok(result);
    }
}
