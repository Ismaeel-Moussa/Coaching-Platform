using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class AthleteOnboardingAssessment
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;

    public OnboardingAssessmentStatus Status { get; set; } = OnboardingAssessmentStatus.Draft;
    public string? PrimaryGoal { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? ActivityLevel { get; set; }
    public string? TrainingExperience { get; set; }
    public int? TrainingDaysPerWeek { get; set; }
    public string AvailableEquipmentJson { get; set; } = "[]";
    public string PreferredTrainingDaysJson { get; set; } = "[]";

    public string? InjuriesOrLimitations { get; set; }
    public string? CurrentPain { get; set; }
    public decimal? AverageSleepHours { get; set; }
    public string? SleepQuality { get; set; }

    public string? FoodAllergies { get; set; }
    public string? FoodIntolerances { get; set; }
    public string? PreferredFoods { get; set; }
    public string? FoodsToAvoid { get; set; }
    public int? TypicalMealsPerDay { get; set; }
    public string? TypicalMealSchedule { get; set; }
    public string? CurrentSupplements { get; set; }
    public string? AdditionalNotes { get; set; }

    public string? CoachReviewNotes { get; set; }
    public int? ReviewedByCoachId { get; set; }
    public Coach? ReviewedByCoach { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReopenReason { get; set; }
    public DateTime? ReopenedAt { get; set; }
    public int? ReopenedByCoachId { get; set; }
    public Coach? ReopenedByCoach { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OnboardingPhoto> Photos { get; set; } = new List<OnboardingPhoto>();
}

public class OnboardingPhoto
{
    public int Id { get; set; }
    public int OnboardingAssessmentId { get; set; }
    public AthleteOnboardingAssessment OnboardingAssessment { get; set; } = null!;
    public PhotoAngle Angle { get; set; }
    public string BlobUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

