namespace JokerNutrition.Business.DTOs.Coach;

public class AthleteSetupReadinessDto
{
    public const int RequiredStepCount = 5;

    public bool AssessmentReviewed { get; set; }
    public bool WorkoutAssigned { get; set; }
    public bool NutritionPlanAssigned { get; set; }
    public bool NutritionTargetsConfigured { get; set; }
    public bool ActivityTargetsConfigured { get; set; }

    public int CompletedRequiredSteps =>
        (AssessmentReviewed ? 1 : 0) +
        (WorkoutAssigned ? 1 : 0) +
        (NutritionPlanAssigned ? 1 : 0) +
        (NutritionTargetsConfigured ? 1 : 0) +
        (ActivityTargetsConfigured ? 1 : 0);

    public int TotalRequiredSteps => RequiredStepCount;
    public bool IsComplete => CompletedRequiredSteps == RequiredStepCount;
}
