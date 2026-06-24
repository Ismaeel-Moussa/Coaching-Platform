using JokerNutrition.Business.Forms.Workouts;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/workouts")]
[Authorize(Roles = "Athlete")]
[ServiceFilter(typeof(Filters.ApiExceptionFilter))]
public class WorkoutLogsController : ControllerBase
{
    private readonly IWorkoutLogService _workoutLogService;

    public WorkoutLogsController(IWorkoutLogService workoutLogService)
    {
        _workoutLogService = workoutLogService;
    }

    /// <summary>
    /// Get today's scheduled workout for the logged-in athlete, including all exercises and any sets already logged.
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var result = await _workoutLogService.GetTodaysWorkoutAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get the athlete's full 6-day PPL program structure.
    /// </summary>
    [HttpGet("program")]
    public async Task<IActionResult> GetProgram()
    {
        var result = await _workoutLogService.GetProgramAsync();
        return Ok(result);
    }

    /// <summary>
    /// Log a completed set (exercise + weight + reps) within an active workout session.
    /// </summary>
    [HttpPost("log-set")]
    public async Task<IActionResult> LogSet([FromBody] LogSetForm form)
    {
        var result = await _workoutLogService.LogSetAsync(form);
        return Ok(result);
    }

    /// <summary>
    /// Mark today's full workout session as completed. Updates athlete's streak counter.
    /// </summary>
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteWorkout([FromBody] CompleteWorkoutForm form)
    {
        await _workoutLogService.CompleteWorkoutAsync(form);
        return Ok(new { message = "Workout completed! Streak updated." });
    }

    /// <summary>
    /// Get exercise history for progressive overload tracking.
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var result = await _workoutLogService.GetHistoryAsync();
        return Ok(result);
    }
}
