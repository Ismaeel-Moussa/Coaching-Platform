using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class ClientCheckIn
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public DateOnly WeekOf { get; set; }
    public DateTime SubmittedAt { get; set; }

    public decimal WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? ThighCm { get; set; }

    public int SleepQuality { get; set; }
    public int EnergyLevel { get; set; }
    public int GutHealth { get; set; }
    public int TrainingStress { get; set; }

    public string? CoachNotes { get; set; }
    public DateTime? CoachReviewedAt { get; set; }

    public ICollection<CheckInPhoto> Photos { get; set; } = new List<CheckInPhoto>();
}

public class CheckInPhoto
{
    public int Id { get; set; }
    public int ClientCheckInId { get; set; }
    public ClientCheckIn CheckIn { get; set; } = null!;
    public PhotoAngle Angle { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}
