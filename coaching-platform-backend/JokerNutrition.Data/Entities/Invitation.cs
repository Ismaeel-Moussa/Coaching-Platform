using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class Invitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = "Athlete";
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public int IssuedByCoachId { get; set; }
    public Coach IssuedBy { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
