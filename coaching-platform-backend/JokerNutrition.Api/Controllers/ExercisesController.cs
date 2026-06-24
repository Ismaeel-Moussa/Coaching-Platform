using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.Services;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ServiceFilter(typeof(Filters.ApiExceptionFilter))]
public class ExercisesController : ControllerBase
{
    private readonly IExerciseService _exerciseService;

    public ExercisesController(IExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    /// <summary>
    /// List all exercises, optionally filtered by name search or muscle group.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetExercises(
        [FromQuery] string? search,
        [FromQuery] MuscleGroup? muscle,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _exerciseService.GetExercisesAsync(search, muscle, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a single exercise by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetExercise(int id)
    {
        var result = await _exerciseService.GetExerciseByIdAsync(id);
        return Ok(result);
    }
}
