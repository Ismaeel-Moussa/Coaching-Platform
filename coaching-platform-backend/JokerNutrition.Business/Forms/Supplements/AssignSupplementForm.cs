using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Supplements;

public class AssignSupplementForm
{
    public int AthleteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SupplementType Type { get; set; }
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
}
