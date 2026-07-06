using System.Net;
using System.Net.Http.Json;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the weekly check-in endpoints.
/// Tests: SubmitCheckIn, GetHistory, GetPending, AddCoachNotes.
/// </summary>
public class CheckInTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public CheckInTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCheckInHistory_WithAthleteToken_Returns200()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/checkins/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingCheckIns_WithAthleteToken_Returns403()
    {
        // The /pending endpoint is Coach/Admin only
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/checkins/pending");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingCheckIns_WithCoachToken_Returns200()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/checkins/pending");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SubmitCheckIn_WithInvalidSliderValues_Returns400()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Slider values must be 1-10; 0 and 11 are invalid
        var payload = new
        {
            weightKg = 80.5,
            sleepQuality = 0,      // invalid
            energyLevel = 11,      // invalid
            gutHealth = 7,
            trainingStress = 6
        };

        var response = await _client.PostAsJsonAsync("/api/checkins", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddCoachNotes_WithAthleteToken_Returns403()
    {
        // Adding coach notes is Coach/Admin only
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.PutAsJsonAsync("/api/checkins/999/coach-notes",
            new { notes = "Great progress!" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
