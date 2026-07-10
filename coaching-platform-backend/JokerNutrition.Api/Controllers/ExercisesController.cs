using JokerNutrition.Business.Common;
using JokerNutrition.Business.DTOs.Workouts;
using JokerNutrition.Business.Forms.Exercises;
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

    /// <summary>List all exercises, optionally filtered by name search or muscle group.</summary>
    [HttpGet]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetExercises(
        [FromQuery] string? search,
        [FromQuery] MuscleGroup? muscleGroup,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _exerciseService.GetExercisesAsync(search, muscleGroup, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a single exercise by ID.</summary>
    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Client)]
    public async Task<IActionResult> GetExercise(int id)
    {
        var result = await _exerciseService.GetExerciseByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Create a new exercise in the global library.</summary>
    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateExerciseForm form)
    {
        var result = await _exerciseService.CreateExerciseAsync(form);
        return Created($"/api/exercises/{result.Id}", result);
    }

    /// <summary>Update an existing exercise (partial — only send changed fields).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExerciseForm form)
    {
        var result = await _exerciseService.UpdateExerciseAsync(id, form);
        return Ok(result);
    }

    /// <summary>Soft-delete an exercise (sets IsActive = false).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _exerciseService.DeleteExerciseAsync(id);
        return NoContent();
    }
}

