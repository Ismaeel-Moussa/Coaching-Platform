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

    private static Athlete CreateAthlete(AthleteOnboardingAssessment? assessment) => new()
    {
        Id = 10,
        User = new User { FirstName = "Test", LastName = "Athlete" },
        OnboardingAssessment = assessment
    };
}
