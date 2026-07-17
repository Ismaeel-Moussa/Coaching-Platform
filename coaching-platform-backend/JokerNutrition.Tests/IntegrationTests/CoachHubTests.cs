using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the Coach Hub endpoints.
/// </summary>
public class CoachHubTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public CoachHubTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboard_WithCoachToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/coach-hub/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("pendingOnboardingAssessmentsCount").GetInt32());
    }

    [Fact]
    public async Task GetDashboard_WithAthleteToken_Returns403()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/coach-hub/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetRoster_WithCoachToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/coach-hub/roster");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRoster_AwaitingAssessmentReviewFilter_Returns200()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/roster?filter=AwaitingAssessmentReview");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = body.RootElement.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal(1, items[0].GetProperty("athleteId").GetInt32());
        Assert.Equal("Submitted", items[0].GetProperty("onboardingStatus").GetString());
    }

    [Theory]
    [InlineData("/api/coach-hub/dashboard")]
    [InlineData("/api/coach-hub/roster?filter=AwaitingAssessmentReview")]
    public async Task CoachHubEndpoints_WithRoleOnlyAdmin_Return200(string path)
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "admin@test.com", "Admin@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
