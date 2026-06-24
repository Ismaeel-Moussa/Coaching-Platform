using JokerNutrition.Business.Forms.Supplements;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(Filters.ApiExceptionFilter))]
public class SupplementsController : ControllerBase
{
    private readonly ISupplementService _supplementService;

    public SupplementsController(ISupplementService supplementService)
    {
        _supplementService = supplementService;
    }

    /// <summary>
    /// Get the athlete's active supplement schedule with today's completion status.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetSchedule()
    {
        var result = await _supplementService.GetScheduleAsync();
        return Ok(result);
    }

    /// <summary>
    /// Toggle a supplement as taken or untaken for a given date.
    /// </summary>
    [HttpPost("log")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> ToggleTaken([FromBody] ToggleSupplementForm form)
    {
        var result = await _supplementService.ToggleTakenAsync(form);
        return Ok(result);
    }

    /// <summary>
    /// Assign a new supplement schedule item to an athlete (Coach or Admin only).
    /// </summary>
    [HttpPost("schedule")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> AssignSupplement([FromBody] AssignSupplementForm form)
    {
        var result = await _supplementService.AssignSupplementAsync(form);
        return Created(string.Empty, result);
    }
}
