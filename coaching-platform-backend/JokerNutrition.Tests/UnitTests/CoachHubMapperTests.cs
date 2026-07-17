using JokerNutrition.Business.Mappers;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Enums;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class CoachHubMapperTests
{
    [Theory]
    [InlineData(OnboardingAssessmentStatus.Draft, "Draft")]
    [InlineData(OnboardingAssessmentStatus.Submitted, "Submitted")]
    [InlineData(OnboardingAssessmentStatus.Reviewed, "Reviewed")]
    public void MapRosterItem_MapsOnboardingStatus(OnboardingAssessmentStatus status, string expected)
    {
        var athlete = CreateAthlete(new AthleteOnboardingAssessment { Status = status });

        var result = CoachHubMapper.MapRosterItem(athlete, null, null, 0);

        Assert.Equal(expected, result.OnboardingStatus);
    }

    [Fact]
    public void MapRosterItem_MapsReopenedDraftAsChangesRequested()
    {
        var athlete = CreateAthlete(new AthleteOnboardingAssessment
        {
            Status = OnboardingAssessmentStatus.Draft,
            ReopenedAt = DateTime.UtcNow
        });

        var result = CoachHubMapper.MapRosterItem(athlete, null, null, 0);

        Assert.Equal("ChangesRequested", result.OnboardingStatus);
    }

    [Fact]
    public void MapRosterItem_MapsMissingAssessmentAsNotStarted()
    {
        var result = CoachHubMapper.MapRosterItem(CreateAthlete(null), null, null, 0);

        Assert.Equal("NotStarted", result.OnboardingStatus);
    }

    [Fact]
    public void MapSetupReadiness_ReturnsZeroOfFive_WhenNothingIsConfigured()
    {
        var result = CoachHubMapper.MapSetupReadiness(CreateAthlete(null), false, false, null);

        Assert.False(result.IsComplete);
        Assert.Equal(0, result.CompletedRequiredSteps);
        Assert.Equal(5, result.TotalRequiredSteps);
    }

    [Fact]
    public void MapSetupReadiness_ReturnsFiveOfFive_WhenAllRequiredSetupExists()
    {
        var athlete = CreateAthlete(new AthleteOnboardingAssessment
        {
            Status = OnboardingAssessmentStatus.Reviewed
        });
        var target = new MacroTarget
        {
            IsActive = true,
            TargetCalories = 2200,
            TargetProtein = 180,
            TargetCarbs = 220,
            TargetFat = 70,
            WaterLitersTarget = 4,
            StepsTarget = 8000
        };

        var result = CoachHubMapper.MapSetupReadiness(athlete, true, true, target);

        Assert.True(result.IsComplete);
        Assert.Equal(5, result.CompletedRequiredSteps);
        Assert.Equal(5, result.TotalRequiredSteps);
    }

    private static Athlete CreateAthlete(AthleteOnboardingAssessment? assessment) => new()
    {
        Id = 10,
        User = new User { FirstName = "Test", LastName = "Athlete" },
        OnboardingAssessment = assessment
    };
}
