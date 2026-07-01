using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Foods;

public class CreateFoodForm
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }   // "Protein" | "Carb" | "Fat" | "Dairy" | "Vegetable" | "Fruit"

    [Range(0, 9000)]
    public decimal CaloriesPer100g { get; set; }

    [Range(0, 900)]
    public decimal ProteinPer100g { get; set; }

    [Range(0, 900)]
    public decimal CarbsPer100g { get; set; }

    [Range(0, 900)]
    public decimal FatPer100g { get; set; }

    [Range(0, 100)]
    public decimal FiberPer100g { get; set; }
}
