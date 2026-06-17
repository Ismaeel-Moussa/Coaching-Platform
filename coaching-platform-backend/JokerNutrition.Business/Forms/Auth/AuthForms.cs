using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Auth;

public class LoginForm
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterForm
{
    [Required, MaxLength(500)]
    public string InvitationToken { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RefreshTokenForm
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordForm
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordForm
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}
