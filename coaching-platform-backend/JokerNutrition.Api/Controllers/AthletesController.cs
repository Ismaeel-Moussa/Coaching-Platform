using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Athletes;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/athletes")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize]
public class AthletesController : ControllerBase
{
    private readonly IAthleteService _athleteService;

    public AthletesController(IAthleteService athleteService)
    {
        _athleteService = athleteService;
    }

    /// <summary>Get athlete dashboard: macros, streak, today's workout status.</summary>
    [HttpGet("me/dashboard")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _athleteService.GetDashboardAsync();
        return Ok(result);
    }

    /// <summary>Get the current athlete's active macro targets set by their coach.</summary>
    [HttpGet("me/targets")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetMyTargets()
    {
        var result = await _athleteService.GetActiveTargetsAsync();
        return Ok(result);
    }

    /// <summary>Get macro targets for a specific athlete (Coach/Admin only).</summary>
    [HttpGet("{id:int}/targets")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetTargetsForAthlete(int id)
    {
        var result = await _athleteService.GetTargetsForAthleteAsync(id);
        return Ok(result);
    }

    /// <summary>Set daily macro targets for an athlete (Coach/Admin only). Deactivates previous targets.</summary>
    [HttpPost("{id:int}/targets")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> SetTargets(int id, [FromBody] SetMacroTargetForm form)
    {
        var result = await _athleteService.SetTargetsAsync(id, form);
        return Created("", result);
    }
}
