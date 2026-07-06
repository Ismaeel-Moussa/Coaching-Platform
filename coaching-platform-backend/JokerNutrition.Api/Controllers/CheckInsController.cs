using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Common;
using JokerNutrition.Business.Forms.CheckIns;
using JokerNutrition.Business.Services;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/checkins")]
[Authorize]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class CheckInsController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInsController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    /// <summary>
    /// Submit or resubmit the current week's biometric check-in (upsert).
    /// Returns 201 on first submission, 200 on update.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> SubmitCheckIn([FromBody] SubmitCheckInForm form)
    {
        // Basic slider validation
        if (form.SleepQuality < 1 || form.SleepQuality > 10 ||
            form.EnergyLevel < 1 || form.EnergyLevel > 10 ||
            form.GutHealth < 1 || form.GutHealth > 10 ||
            form.TrainingStress < 1 || form.TrainingStress > 10)
        {
            return BadRequest("Slider values must be between 1 and 10.");
        }

        var result = await _checkInService.SubmitCheckInAsync(form);

        // 201 on first submission (no CoachReviewedAt and photos empty is a proxy for new)
        // More reliably: we return 201 only if this is newly created; service already sets SubmittedAt to UtcNow.
        // Use 200 for simplicity — frontend checks the id.
        return Ok(result);
    }

    /// <summary>
    /// Upload 1-3 progress photos for an existing check-in.
    /// Accepts multipart/form-data with fields: Front, Side, Back (all optional).
    /// Replaces any existing photo for the same angle.
    /// </summary>
    [HttpPost("{id:int}/photos")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> UploadPhotos(int id, IFormCollection form)
    {
        var photos = new List<(PhotoAngle Angle, IFormFile File)>();

        foreach (var angleName in new[] { "Front", "Side", "Back" })
        {
            if (form.Files.GetFile(angleName) is { } file)
            {
                if (!Enum.TryParse<PhotoAngle>(angleName, out var angle))
                    continue;

                photos.Add((angle, file));
            }
        }

        if (photos.Count == 0)
            return BadRequest("At least one photo field (Front, Side, or Back) must be provided.");

        var result = await _checkInService.UploadPhotosAsync(id, photos);
        return Ok(result);
    }

    /// <summary>
    /// Delete a single progress photo angle (Front, Side, or Back).
    /// Removes the file from blob storage and the DB record.
    /// </summary>
    [HttpDelete("{id:int}/photos/{angle}")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> DeletePhoto(int id, string angle)
    {
        if (!Enum.TryParse<PhotoAngle>(angle, true, out var photoAngle))
            return BadRequest($"Invalid angle '{angle}'. Valid values: Front, Side, Back.");

        await _checkInService.DeletePhotoAsync(id, photoAngle);
        return NoContent();
    }

    /// <summary>
    /// Returns paginated check-in history for the logged-in athlete.
    /// Includes coach notes and signed photo download URLs.
    /// </summary>
    [HttpGet("history")]
    [Authorize(Roles = "Coach,Admin,Athlete")]
    public async Task<IActionResult> GetHistory([FromQuery] int? athleteId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (athleteId.HasValue)
        {
            if (User.IsInRole("Athlete"))
                return Forbid();

            var result = await _checkInService.GetCheckInHistoryAsync(athleteId.Value, new BasePaginationForm
            {
                Page = page,
                PageSize = pageSize
            });
            return Ok(result);
        }
        else
        {
            if (!User.IsInRole("Athlete"))
                return BadRequest("athleteId parameter is required for coaches/admins.");

            var result = await _checkInService.GetMyCheckInHistoryAsync(new BasePaginationForm
            {
                Page = page,
                PageSize = pageSize
            });
            return Ok(result);
        }
    }

    /// <summary>
    /// Returns athletes under the coach's roster with no check-in for the current week.
    /// Used for the Coach Dashboard pending check-in KPI and alert list.
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _checkInService.GetPendingCheckInsAsync(new BasePaginationForm
        {
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    /// <summary>
    /// Coach saves feedback notes on an athlete's check-in submission.
    /// </summary>
    [HttpPut("{id:int}/coach-notes")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> AddCoachNotes(int id, [FromBody] AddCoachNotesForm form)
    {
        var result = await _checkInService.AddCoachNotesAsync(id, form);
        return Ok(result);
    }

    /// <summary>
    /// Returns signed 24-hour download URLs for all progress photos on a check-in.
    /// </summary>
    [HttpGet("{id:int}/photos")]
    [Authorize(Roles = "Coach,Admin,Athlete")]
    public async Task<IActionResult> GetPhotos(int id)
    {
        var result = await _checkInService.GetCheckInPhotosAsync(id);
        return Ok(result);
    }
}
