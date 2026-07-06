using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Entities.Identities;

namespace JokerNutrition.Data.Entities;

public class CoachFeedbackNote
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public string NoteText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Notification
{
    public int Id { get; set; }
    public int RecipientUserId { get; set; }
    public User RecipientUser { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? PerformedByName { get; set; }    // Denormalised for fast reads
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? IpAddress { get; set; }           // Caller's remote IP
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
