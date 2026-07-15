using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Onboarding;
using JokerNutrition.Business.Services;
using JokerNutrition.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/onboarding")]
[Authorize]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingAssessmentService _service;

    public OnboardingController(IOnboardingAssessmentService service)
    {
        _service = service;
    }

    /// <summary>Returns the logged-in athlete's onboarding assessment and completion status.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken) => 
        Ok(await _service.GetMineAsync(cancellationToken));

    /// <summary>Saves the logged-in athlete's onboarding answers as a draft.</summary>
    [HttpPut("me")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> SaveDraft([FromBody] SaveOnboardingAssessmentForm form, CancellationToken cancellationToken) =>
        Ok(await _service.SaveDraftAsync(form, cancellationToken));

    /// <summary>Validates and submits the logged-in athlete's assessment for coach review.</summary>
    [HttpPost("me/submit")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> Submit([FromBody] SaveOnboardingAssessmentForm form, CancellationToken cancellationToken) =>
        Ok(await _service.SubmitAsync(form, cancellationToken));

    /// <summary>Returns an athlete's onboarding assessment for their assigned coach or an admin.</summary>
    [HttpGet("athletes/{athleteId:int}")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> GetForAthlete(int athleteId, CancellationToken cancellationToken) =>
        Ok(await _service.GetForAthleteAsync(athleteId, cancellationToken));

    /// <summary>Marks a submitted athlete assessment as reviewed and saves coach notes.</summary>
    [HttpPut("athletes/{athleteId:int}/review")]
    [Authorize(Roles = "Coach,Admin")]
    public async Task<IActionResult> Review(int athleteId, [FromBody] ReviewOnboardingAssessmentForm form, CancellationToken cancellationToken) =>
        Ok(await _service.ReviewAsync(athleteId, form, cancellationToken));

    /// <summary>Upload 1-3 progress photos for the athlete onboarding assessment.</summary>
    [HttpPost("me/photos")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> UploadPhotos(IFormCollection form, CancellationToken cancellationToken)
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

        var result = await _service.UploadPhotosAsync(photos, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a single onboarding progress photo angle.</summary>
    [HttpDelete("me/photos/{angle}")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> DeletePhoto(string angle, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PhotoAngle>(angle, true, out var photoAngle))
            return BadRequest($"Invalid angle '{angle}'. Valid values: Front, Side, Back.");

        await _service.DeletePhotoAsync(photoAngle, cancellationToken);
        return NoContent();
    }
}
