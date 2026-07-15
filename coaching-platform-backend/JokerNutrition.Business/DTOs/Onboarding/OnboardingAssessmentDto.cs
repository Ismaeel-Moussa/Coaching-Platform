using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.DTOs.Onboarding;

public class OnboardingAssessmentDto
{
    public int? Id { get; set; }
    public int AthleteId { get; set; }
    public string AthleteName { get; set; } = string.Empty;
    public OnboardingAssessmentStatus Status { get; set; }
    public string? PrimaryGoal { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? ActivityLevel { get; set; }
    public string? TrainingExperience { get; set; }
    public int? TrainingDaysPerWeek { get; set; }
    public List<string> AvailableEquipment { get; set; } = [];
    public List<string> PreferredTrainingDays { get; set; } = [];
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
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool HasInjuryFlag { get; set; }
    public bool HasPainFlag { get; set; }
    public bool HasAllergyFlag { get; set; }
    public bool HasFoodRestrictionFlag { get; set; }
    public bool RequiresCompletion => Status is OnboardingAssessmentStatus.NotStarted or OnboardingAssessmentStatus.Draft;
    public List<OnboardingPhotoDto> Photos { get; set; } = [];
}

public class OnboardingPhotoDto
{
    public int Id { get; set; }
    public string Angle { get; set; } = string.Empty;
    public string SignedDownloadUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

