using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Workouts;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/workout-templates")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize(Roles = "Coach,Admin")]
public class WorkoutTemplatesController : ControllerBase
{
    private readonly IWorkoutTemplateService _templateService;

    public WorkoutTemplatesController(IWorkoutTemplateService templateService)
    {
        _templateService = templateService;
    }

    /// <summary>List all workout templates created by the logged-in coach (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _templateService.GetTemplatesAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a single template with all its days and exercises (full nested structure).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTemplateById(int id)
    {
        var result = await _templateService.GetTemplateByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Create a new 6-day workout template.
    /// Called when the coach clicks "Save Template" in the Template Builder.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkoutTemplateForm form)
    {
        var result = await _templateService.CreateTemplateAsync(form);
        return Created($"/api/workout-templates/{result.Id}", result);
    }

    /// <summary>
    /// Replace a template's metadata and all its days/exercises (full replacement).
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateWorkoutTemplateForm form)
    {
        var result = await _templateService.UpdateTemplateAsync(id, form);
        return Ok(result);
    }

    /// <summary>
    /// Assign a template to one or more athletes.
    /// Deactivates any existing active program for each athlete before creating the new one.
    /// </summary>
    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignTemplateForm form)
    {
        var (count, message) = await _templateService.AssignTemplateAsync(id, form);
        return Ok(new { assignedCount = count, message });
    }

    /// <summary>
    /// Soft-delete a workout template (set IsActive = false).
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _templateService.DeleteTemplateAsync(id);
        return NoContent();
    }
}
