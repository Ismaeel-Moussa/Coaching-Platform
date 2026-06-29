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
    private readonly IBlobStorageService _blobStorageService;

    public ProfileController(IProfileService profileService, IBlobStorageService blobStorageService)
    {
        _profileService = profileService;
        _blobStorageService = blobStorageService;
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

    /// <summary>Upload a profile picture/avatar and update User profile.</summary>
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was uploaded." });
        }

        // Validate content type (image only)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new { message = "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed." });
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return BadRequest(new { message = "File size cannot exceed 5MB." });
        }

        using (var stream = file.OpenReadStream())
        {
            var url = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);
            
            // Automatically update user's profile picture url in db
            var currentProfile = await _profileService.GetProfileAsync();
            var updateForm = new UpdateProfileForm
            {
                FirstName = currentProfile.FirstName,
                LastName = currentProfile.LastName,
                ProfilePictureUrl = url,
                Bio = currentProfile.Bio,
                WeightKg = currentProfile.WeightKg,
                HeightCm = currentProfile.HeightCm,
                TargetGoal = currentProfile.TargetGoal
            };
            var updatedUser = await _profileService.UpdateProfileAsync(updateForm);

            return Ok(new { url, user = updatedUser });
        }
    }
}
