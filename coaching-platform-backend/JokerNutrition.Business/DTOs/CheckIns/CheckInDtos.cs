namespace JokerNutrition.Business.DTOs.CheckIns;

public class CheckInPhotoDto
{
    public int Id { get; set; }
    public string Angle { get; set; } = string.Empty;           // "Front" | "Side" | "Back"
    public string SignedDownloadUrl { get; set; } = string.Empty; // 24h Azure Blob SAS URL
    public string UploadedAt { get; set; } = string.Empty;
}

public class CheckInDto
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public string AthleteFullName { get; set; } = string.Empty;
    public string WeekOf { get; set; } = string.Empty;          // "YYYY-MM-DD"
    public string SubmittedAt { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? ThighCm { get; set; }
    public int SleepQuality { get; set; }
    public int EnergyLevel { get; set; }
    public int GutHealth { get; set; }
    public int TrainingStress { get; set; }
    public string? CoachNotes { get; set; }
    public string? CoachReviewedAt { get; set; }
    public List<CheckInPhotoDto> Photos { get; set; } = new();
}

public class PendingCheckInDto
{
    public int AthleteId { get; set; }
    public string AthleteFullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? LastCheckInWeekOf { get; set; }     // null if never submitted
    public int DaysSinceLastCheckIn { get; set; }      // -1 if never submitted
}
