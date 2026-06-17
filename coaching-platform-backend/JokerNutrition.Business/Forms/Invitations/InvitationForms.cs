using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Invitations;

public class CreateInvitationForm
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Athlete"; // "Athlete" | "Coach" | "Admin"

    public int ExpiryHours { get; set; } = 72; // 3 days default
}
