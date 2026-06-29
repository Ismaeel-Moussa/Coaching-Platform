using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Profile;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/profile")]
[ServiceFilter(typeof(ApiExceptionFilter))]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>Get current user profile (athlete metrics or coach bio based on role).</summary>
    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _profileService.GetProfileAsync();
        return Ok(result);
    }

    /// <summary>Update current user profile (first name, last name, image, role-specific details).</summary>
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileForm form)
    {
        var result = await _profileService.UpdateProfileAsync(form);
        return Ok(result);
    }

    /// <summary>Change password for logged-in user.</summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordForm form)
    {
        await _profileService.ChangePasswordAsync(form);
        return Ok(new { message = "Password changed successfully." });
    }
}
