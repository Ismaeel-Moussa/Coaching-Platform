using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.NutritionPlans;
using JokerNutrition.Business.Services;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/nutrition-plans")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize]
public class NutritionPlansController : ControllerBase
{
    private readonly INutritionPlanService _service;

    public NutritionPlansController(INutritionPlanService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] ContentStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20) =>
        Ok(await _service.GetTemplatesAsync(status, search, page, pageSize));

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetTemplate(int id) => Ok(await _service.GetTemplateAsync(id));

    [HttpPost]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Create([FromBody] UpsertNutritionPlanForm form)
    {
        var result = await _service.CreateTemplateAsync(form);
        return Created($"/api/nutrition-plans/{result.Id}", result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpsertNutritionPlanForm form) =>
        Ok(await _service.UpdateTemplateAsync(id, form));

    [HttpGet("{id:int}/validation")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Validate(int id) => Ok(await _service.ValidateTemplateAsync(id));

    [HttpPost("{id:int}/status")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeNutritionPlanStatusForm form) =>
        Ok(await _service.ChangeStatusAsync(id, form));

    [HttpPost("{id:int}/assign")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignNutritionPlanForm form)
    {
        var count = await _service.AssignAsync(id, form);
        return Ok(new { assignedCount = count });
    }

    [HttpGet("athletes/{athleteId:int}/current")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetAthleteCurrent(int athleteId) =>
        Ok(await _service.GetCurrentAssignmentForCoachAsync(athleteId));

    [HttpGet("me/current")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetMyCurrent() => Ok(await _service.GetMyCurrentAssignmentAsync());
}
