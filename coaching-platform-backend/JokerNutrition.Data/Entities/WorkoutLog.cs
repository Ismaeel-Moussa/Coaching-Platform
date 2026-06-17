using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class ClientProgram
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int AssignedByCoachId { get; set; }
    public Coach AssignedByCoach { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WorkoutLog
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int WorkoutTemplateDayId { get; set; }
    public WorkoutTemplateDay Day { get; set; } = null!;
    public DateOnly Date { get; set; }
    public WorkoutStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<ExerciseSetLog> Sets { get; set; } = new List<ExerciseSetLog>();
}

public class ExerciseSetLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
    public bool IsCompleted { get; set; } = false;
}
