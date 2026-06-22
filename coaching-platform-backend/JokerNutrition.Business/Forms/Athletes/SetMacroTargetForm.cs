namespace JokerNutrition.Business.Forms.Athletes;

public class SetMacroTargetForm
{
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }
    public decimal WaterLitersTarget { get; set; } = 4.0m;
    public int StepsTarget { get; set; } = 7000;
}
