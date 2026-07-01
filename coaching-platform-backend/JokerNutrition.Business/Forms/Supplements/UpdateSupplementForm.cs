using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Supplements;

public class UpdateSupplementForm
{
    public string Name { get; set; } = string.Empty;
    public SupplementType Type { get; set; }
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
}
