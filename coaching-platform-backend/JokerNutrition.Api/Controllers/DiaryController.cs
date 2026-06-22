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

    /// <summary>Remove a food log entry from the diary.</summary>
    [HttpDelete("log/{id:int}")]
    public async Task<IActionResult> RemoveEntry(int id)
    {
        await _mealLogService.RemoveLogEntryAsync(id);
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
