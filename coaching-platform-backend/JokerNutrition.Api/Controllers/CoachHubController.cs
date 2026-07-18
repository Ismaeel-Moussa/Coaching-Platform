using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Coach;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/coach-hub")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize(Roles = "Coach,Admin")]
public class CoachHubController : ControllerBase
{
    private readonly ICoachHubService _coachHubService;
    private readonly IAthleteProgressReportService _progressReportService;

    public CoachHubController(
        ICoachHubService coachHubService,
        IAthleteProgressReportService progressReportService)
    {
        _coachHubService = coachHubService;
        _progressReportService = progressReportService;
    }

    /// <summary>
    /// Returns KPI summary for the coach dashboard:
    /// active athlete count, average workout completion %, pending check-in and
    /// onboarding-review counts, and the 10 most recent workout feed events.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _coachHubService.GetDashboardAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns all pending coach tasks across the requesting coach's roster.
    /// Supports filtering by task type, priority, athlete name, and pagination.
    /// </summary>
    [HttpGet("action-items")]
    public async Task<IActionResult> GetActionItems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? type = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null)
    {
        var result = await _coachHubService.GetActionItemsAsync(page, pageSize, type, priority, search);
        return Ok(result);
    }

    /// <summary>
    /// Returns a paginated list of recent workout events (InProgress / Completed / Missed)
    /// across all athletes assigned to the logged-in coach, ordered newest first.
    /// Auto-refreshes every 30 seconds on the frontend.
    /// </summary>
    [HttpGet("live-feed")]
    public async Task<IActionResult> GetLiveFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _coachHubService.GetLiveFeedAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Returns per-athlete macro and calorie compliance status for today.
    /// Athletes exceeding calorie targets by more than 5% have IsOverCalorieTarget = true.
    /// </summary>
    [HttpGet("compliance")]
    public async Task<IActionResult> GetCompliance()
    {
        var result = await _coachHubService.GetComplianceRosterAsync();
        return Ok(result);
    }

    /// <summary>
    /// Returns a paginated roster of all athletes assigned to the coach,
    /// with their active program, compliance %, last check-in date, onboarding status,
    /// and status badge. Optional filter: "ComplianceAlert" | "NoRecentCheckIn" |
    /// "AwaitingAssessmentReview" (default: All).
    /// </summary>
    [HttpGet("roster")]
    public async Task<IActionResult> GetRoster(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? filter = null)
    {
        var result = await _coachHubService.GetRosterAsync(page, pageSize, filter);
        return Ok(result);
    }

    /// <summary>
    /// Returns the full deep profile for a single athlete:
    /// biometrics, current macro targets, weight history chart data,
    /// and all coach feedback notes.
    /// Only accessible if the athlete belongs to the requesting coach's roster.
    /// </summary>
    [HttpGet("athletes/{id:int}")]
    public async Task<IActionResult> GetAthleteDeepProfile(int id)
    {
        var result = await _coachHubService.GetAthleteDeepProfileAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Returns an athlete progress report for the selected 4, 8, or 12 week period.
    /// Coach notes are opt-in because they may contain sensitive data.
    /// Coach notes and progress photos are opt-in at the API boundary.
    /// </summary>
    [HttpGet("athletes/{id:int}/progress-report")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetAthleteProgressReport(
        int id,
        [FromQuery] int weeks = 8,
        [FromQuery] bool includeCoachNotes = false,
        [FromQuery] bool includePhotos = false)
    {
        var result = await _progressReportService.GetReportAsync(
            id, weeks, includeCoachNotes, includePhotos, cancellationToken: HttpContext.RequestAborted);
        return Ok(result);
    }

    /// <summary>
    /// Generates and downloads the athlete progress report as a branded PDF document.
    /// </summary>
    [HttpGet("athletes/{id:int}/progress-report/pdf")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> DownloadAthleteProgressReport(
        int id,
        [FromQuery] int weeks = 8,
        [FromQuery] bool includeCoachNotes = false,
        [FromQuery] bool includePhotos = false,
        [FromQuery] string language = "en")
    {
        language = language.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "ar" : "en";
        var report = await _progressReportService.GetReportAsync(
            id, weeks, includeCoachNotes, includePhotos, cancellationToken: HttpContext.RequestAborted);
        var pdf = await _progressReportService.GeneratePdfAsync(
            report, includeCoachNotes, includePhotos, language, cancellationToken: HttpContext.RequestAborted);
        var safeName = string.Join('-', report.AthleteName
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => new string(part.Where(char.IsLetterOrDigit).ToArray()))
            .Where(part => part.Length > 0));
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = $"athlete-{id}";
        return File(pdf, "application/pdf", $"{safeName}-progress-report-{report.PeriodEnd}.pdf");
    }

    /// <summary>
    /// Saves a new coach feedback note for the specified athlete
    /// and triggers an in-app notification to the athlete.
    /// </summary>
    [HttpPost("athletes/{id:int}/notes")]
    public async Task<IActionResult> SaveFeedbackNote(int id, [FromBody] SaveFeedbackNoteForm form)
    {
        var result = await _coachHubService.SaveFeedbackNoteAsync(id, form);
        return Created("", result);
    }

    /// <summary>
    /// Returns a time-series of weight measurements for the athlete
    /// sourced from their weekly check-ins, ordered oldest to newest.
    /// Used for the weight trend line chart in the client detail view.
    /// </summary>
    [HttpGet("athletes/{id:int}/weight-history")]
    public async Task<IActionResult> GetWeightHistory(int id)
    {
        var result = await _coachHubService.GetWeightHistoryAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Create or update nutritional/biometric targets for the specified athlete.
    /// </summary>
    [HttpPut("athletes/{id:int}/targets")]
    public async Task<IActionResult> SaveAthleteTargets(int id, [FromBody] SetMacroTargetsForm form)
    {
        await _coachHubService.SetMacroTargetsAsync(id, form);
        return NoContent();
    }
}
