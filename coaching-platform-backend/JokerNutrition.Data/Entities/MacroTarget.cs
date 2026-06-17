namespace JokerNutrition.Data.Entities;

public class MacroTarget
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int SetByCoachId { get; set; }
    public Coach SetByCoach { get; set; } = null!;
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }
    public decimal WaterLitersTarget { get; set; } = 4.0m;
    public int StepsTarget { get; set; } = 7000;
    public bool IsActive { get; set; } = true;
    public DateTime SetAt { get; set; }
}
