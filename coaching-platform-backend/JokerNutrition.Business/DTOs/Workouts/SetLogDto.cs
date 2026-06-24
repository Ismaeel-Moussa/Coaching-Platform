namespace JokerNutrition.Business.DTOs.Workouts;

public class SetLogDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int SetNumber { get; set; }
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
    public bool IsCompleted { get; set; }
}
