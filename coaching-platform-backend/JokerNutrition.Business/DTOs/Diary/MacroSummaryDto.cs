namespace JokerNutrition.Business.DTOs.Diary;

public class MacroSummaryDto
{
    public DateOnly Date { get; set; }

    // Consumed today
    public decimal CaloriesConsumed { get; set; }
    public decimal ProteinConsumed { get; set; }
    public decimal CarbsConsumed { get; set; }
    public decimal FatConsumed { get; set; }

    // Targets
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }

    // Remaining (can be negative if over)
    public decimal CaloriesRemaining => TargetCalories - CaloriesConsumed;
    public decimal ProteinRemaining => TargetProtein - ProteinConsumed;
    public decimal CarbsRemaining => TargetCarbs - CarbsConsumed;
    public decimal FatRemaining => TargetFat - FatConsumed;

    // Hydration & steps
    public decimal WaterLitersConsumed { get; set; }
    public decimal WaterLitersTarget { get; set; }
    public int StepsWalked { get; set; }
    public int StepsTarget { get; set; }
}
