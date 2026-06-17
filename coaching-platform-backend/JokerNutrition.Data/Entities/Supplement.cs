using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class SupplementSchedule
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public SupplementType Type { get; set; }
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SupplementLog> Logs { get; set; } = new List<SupplementLog>();
}

public class SupplementLog
{
    public int Id { get; set; }
    public int SupplementScheduleId { get; set; }
    public SupplementSchedule Schedule { get; set; } = null!;
    public DateOnly Date { get; set; }
    public bool IsTaken { get; set; } = false;
    public DateTime? TakenAt { get; set; }
}
