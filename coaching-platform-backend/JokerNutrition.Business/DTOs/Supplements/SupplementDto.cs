namespace JokerNutrition.Business.DTOs.Supplements;

public class SupplementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;             // "Essential" | "Optional"
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
    public bool IsTakenToday { get; set; }
    public DateTime? TakenAt { get; set; }
}
