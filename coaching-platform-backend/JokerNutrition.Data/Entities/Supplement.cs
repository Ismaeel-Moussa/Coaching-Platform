using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class SupplementSchedule
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int? SupplementCatalogItemId { get; set; }
    public SupplementCatalogItem? CatalogItem { get; set; }
    public string Name { get; set; } = string.Empty;
    public SupplementType Type { get; set; }
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SupplementLog> Logs { get; set; } = new List<SupplementLog>();
}

public class SupplementCatalogItem
{
    public int Id { get; set; }
    public string SeedKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public SupplementType Type { get; set; }
    public string? Education { get; set; }
    public string? EducationAr { get; set; }
    public string? SafetyWarning { get; set; }
    public string? SafetyWarningAr { get; set; }
    public bool RequiresClinicianApproval { get; set; }
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Draft;
    public int ContentVersion { get; set; } = 1;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<SupplementSchedule> Schedules { get; set; } = new List<SupplementSchedule>();
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
