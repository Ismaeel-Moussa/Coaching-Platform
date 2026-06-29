using System.Security.Principal;
using JokerNutrition.Business.DTOs.Auth;
using JokerNutrition.Business.DTOs.Profile;
using JokerNutrition.Business.Forms.Profile;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync();
    Task<UserInfoDto> UpdateProfileAsync(UpdateProfileForm form);
    Task ChangePasswordAsync(ChangePasswordForm form);
}

public class ProfileService : _BaseService, IProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly IAthleteRepository _athleteRepo;
    private readonly ICoachRepository _coachRepo;

    public ProfileService(
        IPrincipal principal,
        ILogger<ProfileService> logger,
        UserManager<User> userManager,
        IAthleteRepository athleteRepo,
        ICoachRepository coachRepo)
        : base(principal, logger)
    {
        _userManager = userManager;
        _athleteRepo = athleteRepo;
        _coachRepo = coachRepo;
    }

    public async Task<UserProfileDto> GetProfileAsync()
    {
        var userId = LoggedInUser.Id;
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        var role = LoggedInUser.Role;

        var dto = new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };

        if (role == "Athlete")
        {
            var athlete = await _athleteRepo.Query()
                .Include(a => a.AssignedCoach).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (athlete != null)
            {
                dto.WeightKg = athlete.WeightKg;
                dto.HeightCm = athlete.HeightCm;
                dto.TargetGoal = athlete.TargetGoal;
                dto.CurrentStreak = athlete.CurrentStreak;
                dto.LongestStreak = athlete.LongestStreak;
                if (athlete.AssignedCoach != null)
                {
                    dto.AssignedCoachName = $"{athlete.AssignedCoach.User.FirstName} {athlete.AssignedCoach.User.LastName}";
                }
            }
        }
        else if (role == "Coach" || role == "Admin")
        {
            var coach = await _coachRepo.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (coach != null)
            {
                dto.Bio = coach.Bio;
            }
        }

        return dto;
    }

    public async Task<UserInfoDto> UpdateProfileAsync(UpdateProfileForm form)
    {
        var userId = LoggedInUser.Id;
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        user.FirstName = form.FirstName;
        user.LastName = form.LastName;
        user.ProfilePictureUrl = form.ProfilePictureUrl;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var role = LoggedInUser.Role;

        if (role == "Athlete")
        {
            var athlete = await _athleteRepo.Query()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (athlete != null)
            {
                athlete.WeightKg = form.WeightKg;
                athlete.HeightCm = form.HeightCm;
                athlete.TargetGoal = form.TargetGoal;
                _athleteRepo.Update(athlete);
                await _athleteRepo.SaveChangesAsync();
            }
        }
        else if (role == "Coach" || role == "Admin")
        {
            var coach = await _coachRepo.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (coach != null)
            {
                coach.Bio = form.Bio;
                _coachRepo.Update(coach);
                await _coachRepo.SaveChangesAsync();
            }
        }

        return new UserInfoDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            ProfilePictureUrl = user.ProfilePictureUrl
        };
    }

    public async Task ChangePasswordAsync(ChangePasswordForm form)
    {
        if (form.NewPassword != form.ConfirmPassword)
        {
            throw new ArgumentException("New password and confirm password do not match.");
        }

        var userId = LoggedInUser.Id;
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, form.CurrentPassword, form.NewPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
