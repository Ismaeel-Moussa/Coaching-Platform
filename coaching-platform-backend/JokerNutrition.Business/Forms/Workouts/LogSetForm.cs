namespace JokerNutrition.Business.Forms.Workouts;

public class LogSetForm
{
    public int WorkoutLogId { get; set; }
    public int ExerciseId { get; set; }
    public int SetNumber { get; set; }
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
}
