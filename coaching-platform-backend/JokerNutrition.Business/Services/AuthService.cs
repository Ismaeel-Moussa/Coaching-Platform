using System.Security.Principal;
using JokerNutrition.Business.Configurations;
using JokerNutrition.Business.DTOs.Auth;
using JokerNutrition.Data.Enums;
using JokerNutrition.Business.Forms.Auth;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JokerNutrition.Business.Services;

public interface IAuthService
{
    Task<AuthTokenDto> LoginAsync(LoginForm form);
    Task<AuthTokenDto> RegisterAsync(RegisterForm form);
    Task<AuthTokenDto> RefreshTokenAsync(RefreshTokenForm form);
    Task ForgotPasswordAsync(ForgotPasswordForm form);
    Task ResetPasswordAsync(ResetPasswordForm form);
}

public class AuthService : _BaseService, IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtTokenHelper _jwtHelper;
    private readonly IInvitationRepository _invitationRepo;
    private readonly IPasswordResetTokenRepository _tokenRepo;
    private readonly IAthleteRepository _athleteRepo;
    private readonly ICoachRepository _coachRepo;
    private readonly IEmailService _emailService;
    private readonly AppSettings _appSettings;

    public AuthService(
        IPrincipal principal,
        ILogger<AuthService> logger,
        UserManager<User> userManager,
        IJwtTokenHelper jwtHelper,
        IInvitationRepository invitationRepo,
        IPasswordResetTokenRepository tokenRepo,
        IAthleteRepository athleteRepo,
        ICoachRepository coachRepo,
        IEmailService emailService,
        IOptions<AppSettings> appSettings)
        : base(principal, logger)
    {
        _userManager = userManager;
        _jwtHelper = jwtHelper;
        _invitationRepo = invitationRepo;
        _tokenRepo = tokenRepo;
        _athleteRepo = athleteRepo;
        _coachRepo = coachRepo;
        _emailService = emailService;
        _appSettings = appSettings.Value;
    }

    public async Task<AuthTokenDto> LoginAsync(LoginForm form)
    {
        var user = await _userManager.FindByEmailAsync(form.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        var isValid = await _userManager.CheckPasswordAsync(user, form.Password);
        if (!isValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Athlete";

        return await IssueTokenPairAsync(user, role);
    }

    public async Task<AuthTokenDto> RegisterAsync(RegisterForm form)
    {
        if (form.Password != form.ConfirmPassword)
            throw new ArgumentException("Passwords do not match.");

        // Validate invitation token
        var invitation = await _invitationRepo.Query()
            .FirstOrDefaultAsync(i => i.Token == form.InvitationToken
                && i.Status == InvitationStatus.Pending
                && i.ExpiresAt > DateTime.UtcNow)
            ?? throw new ArgumentException("Invalid or expired invitation token.");

        // Create user
        var user = new User
        {
            UserName = invitation.Email,
            Email = invitation.Email,
            FirstName = form.FirstName,
            LastName = form.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, form.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, invitation.Role);

        // Create profile entity based on role
        if (invitation.Role == "Athlete")
        {
            var athlete = new Athlete
            {
                UserId = user.Id,
                AssignedCoachId = invitation.IssuedByCoachId > 0 ? invitation.IssuedByCoachId : null
            };
            await _athleteRepo.CreateAsync(athlete);
            await _athleteRepo.SaveChangesAsync();
        }
        else if (invitation.Role == "Coach")
        {
            var coach = new Coach { UserId = user.Id };
            await _coachRepo.CreateAsync(coach);
            await _coachRepo.SaveChangesAsync();
        }

        // Mark invitation as accepted
        invitation.Status = InvitationStatus.Accepted;
        _invitationRepo.Update(invitation);
        await _invitationRepo.SaveChangesAsync();

        _logger.LogInformation("User {Email} registered with role {Role}", user.Email, invitation.Role);

        return await IssueTokenPairAsync(user, invitation.Role);
    }

    public async Task<AuthTokenDto> RefreshTokenAsync(RefreshTokenForm form)
    {
        var storedToken = await _tokenRepo.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == form.RefreshToken &&
                t.TokenType == "RefreshToken" &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        storedToken.IsUsed = true;
        _tokenRepo.Update(storedToken);
        await _tokenRepo.SaveChangesAsync();

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Athlete";

        return await IssueTokenPairAsync(user, role);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordForm form)
    {
        var user = await _userManager.FindByEmailAsync(form.Email);
        if (user == null) return; // Don't reveal whether email exists

        var resetToken = _jwtHelper.GenerateRefreshToken();
        var tokenEntity = new PasswordResetToken
        {
            UserId = user.Id,
            Token = resetToken,
            TokenType = "PasswordReset",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };
        await _tokenRepo.CreateAsync(tokenEntity);
        await _tokenRepo.SaveChangesAsync();

        var resetUrl = $"{_appSettings.ResetPasswordPageUrl}?token={resetToken}";
        await _emailService.SendPasswordResetEmailAsync(user.Email!, resetUrl);
    }

    public async Task ResetPasswordAsync(ResetPasswordForm form)
    {
        if (form.NewPassword != form.ConfirmPassword)
            throw new ArgumentException("Passwords do not match.");

        var storedToken = await _tokenRepo.Query()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == form.Token &&
                t.TokenType == "PasswordReset" &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow)
            ?? throw new ArgumentException("Invalid or expired reset token.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(storedToken.User);
        var result = await _userManager.ResetPasswordAsync(storedToken.User, token, form.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        storedToken.IsUsed = true;
        _tokenRepo.Update(storedToken);
        await _tokenRepo.SaveChangesAsync();
    }

    // ─── Private helpers ───────────────────────────────────────────────
    private async Task<AuthTokenDto> IssueTokenPairAsync(User user, string role)
    {
        var expiresAt = DateTime.UtcNow.AddDays(1);
        var accessToken = _jwtHelper.GenerateAccessToken(user.Id, user.Email!, role, user.FirstName);
        var refreshToken = _jwtHelper.GenerateRefreshToken();

        var tokenEntity = new PasswordResetToken
        {
            UserId = user.Id,
            Token = refreshToken,
            TokenType = "RefreshToken",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _tokenRepo.CreateAsync(tokenEntity);
        await _tokenRepo.SaveChangesAsync();

        return new AuthTokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = role,
                ProfilePictureUrl = user.ProfilePictureUrl
            }
        };
    }
}
