using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Onboarding;

public class SaveOnboardingAssessmentForm
{
    [MaxLength(100)] public string? PrimaryGoal { get; set; }
    [Range(25, 400)] public decimal? WeightKg { get; set; }
    [Range(100, 250)] public decimal? HeightCm { get; set; }
    [MaxLength(30)] public string? ActivityLevel { get; set; }
    [MaxLength(30)] public string? TrainingExperience { get; set; }
    [Range(1, 7)] public int? TrainingDaysPerWeek { get; set; }
    public List<string> AvailableEquipment { get; set; } = [];
    public List<string> PreferredTrainingDays { get; set; } = [];
    [MaxLength(2000)] public string? InjuriesOrLimitations { get; set; }
    [MaxLength(2000)] public string? CurrentPain { get; set; }
    [Range(0, 24)] public decimal? AverageSleepHours { get; set; }
    [MaxLength(30)] public string? SleepQuality { get; set; }
    [MaxLength(2000)] public string? FoodAllergies { get; set; }
    [MaxLength(2000)] public string? FoodIntolerances { get; set; }
    [MaxLength(2000)] public string? PreferredFoods { get; set; }
    [MaxLength(2000)] public string? FoodsToAvoid { get; set; }
    [Range(1, 10)] public int? TypicalMealsPerDay { get; set; }
    [MaxLength(1000)] public string? TypicalMealSchedule { get; set; }
    [MaxLength(2000)] public string? CurrentSupplements { get; set; }
    [MaxLength(3000)] public string? AdditionalNotes { get; set; }
}

public class ReviewOnboardingAssessmentForm
{
    [MaxLength(3000)] public string? CoachReviewNotes { get; set; }
}
