using JokerNutrition.Api.Filters;
using JokerNutrition.Business.Forms.Auth;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace JokerNutrition.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Login with email and password. Returns JWT access + refresh tokens.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginForm form)
    {
        var result = await _authService.LoginAsync(form);
        return Ok(result);
    }

    /// <summary>Register via invitation token. Creates user + role profile.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterForm form)
    {
        var result = await _authService.RegisterAsync(form);
        return Created("", result);
    }

    /// <summary>Rotate refresh token and return new JWT pair.</summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenForm form)
    {
        var result = await _authService.RefreshTokenAsync(form);
        return Ok(result);
    }

    /// <summary>Send password reset email (mocked in dev).</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordForm form)
    {
        await _authService.ForgotPasswordAsync(form);
        return Ok(new { message = "If the email exists, a reset link has been sent." });
    }

    /// <summary>Reset password using token received by email.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordForm form)
    {
        await _authService.ResetPasswordAsync(form);
        return Ok(new { message = "Password has been reset successfully." });
    }
}
