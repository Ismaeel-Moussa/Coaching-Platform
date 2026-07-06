namespace JokerNutrition.Business.Forms.CheckIns;

public class SubmitCheckInForm
{
    public decimal WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? ThighCm { get; set; }

    public int SleepQuality { get; set; }    // 1-10
    public int EnergyLevel { get; set; }     // 1-10
    public int GutHealth { get; set; }       // 1-10
    public int TrainingStress { get; set; }  // 1-10
}
