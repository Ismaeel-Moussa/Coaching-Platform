namespace JokerNutrition.Business.DTOs.Athletes;

public class MacroTargetDto
{
    public int Id { get; set; }
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }
    public decimal WaterLitersTarget { get; set; }
    public int StepsTarget { get; set; }
    public DateTime SetAt { get; set; }
    public string SetByCoachName { get; set; } = string.Empty;
}
