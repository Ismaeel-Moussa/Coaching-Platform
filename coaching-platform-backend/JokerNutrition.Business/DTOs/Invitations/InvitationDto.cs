using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Invitations;

public class InvitationDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string InviteUrl { get; set; } = string.Empty;
}
