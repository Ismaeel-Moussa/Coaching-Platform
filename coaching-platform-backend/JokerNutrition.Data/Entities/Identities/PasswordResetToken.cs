namespace JokerNutrition.Data.Entities.Identities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "PasswordReset"; // "PasswordReset" | "RefreshToken"
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
