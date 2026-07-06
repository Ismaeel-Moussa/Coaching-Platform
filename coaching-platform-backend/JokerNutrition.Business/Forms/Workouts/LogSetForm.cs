using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Workouts;

public class LogSetForm
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "WorkoutLogId must be greater than 0.")]
    public int WorkoutLogId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ExerciseId must be greater than 0.")]
    public int ExerciseId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "SetNumber must be greater than 0.")]
    public int SetNumber { get; set; }

    [Required]
    [Range(0.0, double.MaxValue, ErrorMessage = "WeightKg must be non-negative.")]
    public decimal WeightKg { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Reps must be greater than 0.")]
    public int Reps { get; set; }
}
