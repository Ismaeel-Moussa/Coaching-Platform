using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Diary;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/diary")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize(Roles = "Athlete")]
public class DiaryController : ControllerBase
{
    private readonly IDiaryService _diaryService;
    private readonly IMealLogService _mealLogService;

    public DiaryController(IDiaryService diaryService, IMealLogService mealLogService)
    {
        _diaryService = diaryService;
        _mealLogService = mealLogService;
    }

    /// <summary>Get full diary for a specific date, grouped by meal type.</summary>
    [HttpGet("{date}")]
    public async Task<IActionResult> GetDiary(DateOnly date)
    {
        var result = await _diaryService.GetDiaryAsync(date);
        return Ok(result);
    }

    /// <summary>Get aggregated macro summary (consumed vs. target) for a date.</summary>
    [HttpGet("summary/{date}")]
    public async Task<IActionResult> GetMacroSummary(DateOnly date)
    {
        var result = await _diaryService.GetMacroSummaryAsync(date);
        return Ok(result);
    }

    /// <summary>Log a food or recipe item to a diary meal slot.</summary>
    [HttpPost("log")]
    public async Task<IActionResult> LogFood([FromBody] LogFoodForm form)
    {
        var result = await _mealLogService.LogFoodAsync(form);
        return Created("", result);
    }

    /// <summary>Log several foods and/or recipes to one meal in a single operation.</summary>
    [HttpPost("log/bulk")]
    public async Task<IActionResult> BulkLogFoods([FromBody] BulkLogFoodForm form)
    {
        var result = await _mealLogService.BulkLogFoodsAsync(form);
        return Created("", result);
    }

    /// <summary>Log one selected option from the athlete's assigned nutrition plan.</summary>
    [HttpPost("log/nutrition-plan")]
    public async Task<IActionResult> LogNutritionPlanOption([FromBody] LogNutritionPlanOptionForm form)
    {
        var result = await _mealLogService.LogNutritionPlanOptionAsync(form);
        return Created("", result);
    }

    /// <summary>Get assigned-plan meal options completed on a specific diary date.</summary>
    [HttpGet("nutrition-plan/{assignmentId:int}/{date}")]
    public async Task<IActionResult> GetNutritionPlanEntries(int assignmentId, DateOnly date)
    {
        var result = await _mealLogService.GetNutritionPlanEntriesAsync(assignmentId, date);
        return Ok(result);
    }

    /// <summary>Get recent, frequent, or favorite foods or recipes for the current athlete.</summary>
    [HttpGet("filters")]
    public async Task<IActionResult> GetFilteredItems([FromQuery] string type, [FromQuery] string source)
    {
        var result = await _mealLogService.GetFilteredItemsAsync(type, source);
        return Ok(result);
    }

    /// <summary>Add or remove a food from the current athlete's favorites.</summary>
    [HttpPost("favorites/food/{id:int}/toggle")]
    public async Task<IActionResult> ToggleFavoriteFood(int id)
    {
        var isFavorite = await _mealLogService.ToggleFavoriteFoodAsync(id);
        return Ok(new { isFavorite });
    }

    /// <summary>Add or remove a recipe from the current athlete's favorites.</summary>
    [HttpPost("favorites/recipe/{id:int}/toggle")]
    public async Task<IActionResult> ToggleFavoriteRecipe(int id)
    {
        var isFavorite = await _mealLogService.ToggleFavoriteRecipeAsync(id);
        return Ok(new { isFavorite });
    }

    /// <summary>Remove a food log entry from the diary.</summary>
    [HttpDelete("log/{id:int}")]
    public async Task<IActionResult> RemoveEntry(int id)
    {
        await _mealLogService.RemoveLogEntryAsync(id);
        return NoContent();
    }

    /// <summary>Remove a complete assigned-plan meal and all of its diary items.</summary>
    [HttpDelete("nutrition-plan/{entryId:int}")]
    public async Task<IActionResult> RemoveNutritionPlanEntry(int entryId)
    {
        await _mealLogService.RemoveNutritionPlanEntryAsync(entryId);
        return NoContent();
    }

    /// <summary>Update water consumed (liters) for the day.</summary>
    [HttpPatch("{date}/water")]
    public async Task<IActionResult> UpdateWater(DateOnly date, [FromBody] UpdateWaterForm form)
    {
        await _diaryService.UpdateWaterAsync(date, form);
        return NoContent();
    }

    /// <summary>Update steps walked for the day.</summary>
    [HttpPatch("{date}/steps")]
    public async Task<IActionResult> UpdateSteps(DateOnly date, [FromBody] UpdateStepsForm form)
    {
        await _diaryService.UpdateStepsAsync(date, form);
        return NoContent();
    }
}
