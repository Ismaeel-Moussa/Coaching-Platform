namespace JokerNutrition.Business.DTOs.Foods;

public class FoodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Category { get; set; }
    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }
    public decimal FiberPer100g { get; set; }
    public bool IsCustom { get; set; }
    public bool IsFavorite { get; set; }
}
