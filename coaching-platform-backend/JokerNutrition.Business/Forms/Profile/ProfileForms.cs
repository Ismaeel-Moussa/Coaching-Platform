using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Profile;

public class UpdateProfileForm
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    // Coach specific
    public string? Bio { get; set; }

    // Athlete specific
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? TargetGoal { get; set; }
}

public class ChangePasswordForm
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;
}
