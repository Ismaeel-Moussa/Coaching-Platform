namespace JokerNutrition.Business.DTOs.Profile;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    // Coach details
    public string? Bio { get; set; }

    // Athlete details
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? TargetGoal { get; set; }
    public int? CurrentStreak { get; set; }
    public int? LongestStreak { get; set; }
    public string? AssignedCoachName { get; set; }
}
