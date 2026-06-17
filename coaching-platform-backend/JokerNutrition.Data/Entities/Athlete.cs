using JokerNutrition.Data.Entities.Identities;

namespace JokerNutrition.Data.Entities;

public class Athlete
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? AssignedCoachId { get; set; }
    public Coach? AssignedCoach { get; set; }

    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? TargetGoal { get; set; }
    public bool RamadanModeEnabled { get; set; } = false;

    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime? LastWorkoutDate { get; set; }

    public ICollection<DailyDiary> Diaries { get; set; } = new List<DailyDiary>();
    public ICollection<ClientProgram> Programs { get; set; } = new List<ClientProgram>();
    public ICollection<ClientCheckIn> CheckIns { get; set; } = new List<ClientCheckIn>();
    public ICollection<MacroTarget> MacroTargets { get; set; } = new List<MacroTarget>();
    public ICollection<SupplementSchedule> SupplementSchedules { get; set; } = new List<SupplementSchedule>();
    public ICollection<CoachFeedbackNote> FeedbackNotes { get; set; } = new List<CoachFeedbackNote>();
}
