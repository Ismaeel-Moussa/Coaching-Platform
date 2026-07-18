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

    [Theory]
    [InlineData("/api/checkins/history?athleteId=3")]
    [InlineData("/api/checkins/2")]
    [InlineData("/api/checkins/2/photos")]
    public async Task ReadAnotherCoachsCheckIns_WithCoachToken_Returns403(string path)
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddNotesToAnotherCoachsCheckIn_WithCoachToken_Returns403()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.PutAsJsonAsync("/api/checkins/2/coach-notes", new { notes = "Not allowed" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ReadAnotherAthletesCheckIn_WithAthleteToken_Returns403()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/checkins/2");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPrivatePhoto_WithAssignedCoach_ReturnsReadOnlyShortLivedSasAndNoStore()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "other-coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/checkins/2/photos");
        var photos = await response.Content.ReadFromJsonAsync<List<PhotoResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        var url = Assert.Single(photos!).SignedDownloadUrl;
        var query = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
        Assert.Equal("r", query["sp"]);
        Assert.Contains("private", query["rscc"] ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        var expiry = DateTimeOffset.Parse(query["se"]!, System.Globalization.CultureInfo.InvariantCulture);
        Assert.InRange(expiry, DateTimeOffset.UtcNow.AddMinutes(10), DateTimeOffset.UtcNow.AddMinutes(16));
    }

    [Fact]
    public async Task GetPrivatePhoto_WithAdmin_Returns200()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "admin@test.com", "Admin@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/checkins/2/photos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record PhotoResponse(string SignedDownloadUrl);
}
