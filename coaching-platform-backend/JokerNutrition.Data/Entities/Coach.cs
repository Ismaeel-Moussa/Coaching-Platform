using JokerNutrition.Data.Entities.Identities;

namespace JokerNutrition.Data.Entities;

public class Coach
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Athlete> Athletes { get; set; } = new List<Athlete>();
    public ICollection<WorkoutTemplate> Templates { get; set; } = new List<WorkoutTemplate>();
    public ICollection<CoachFeedbackNote> FeedbackNotes { get; set; } = new List<CoachFeedbackNote>();
}
