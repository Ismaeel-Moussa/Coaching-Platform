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
        var actionItems = body.RootElement.GetProperty("actionItems");
        Assert.Equal(2, actionItems.GetArrayLength());
        Assert.Equal("AssessmentReview", actionItems[0].GetProperty("type").GetString());
        Assert.Equal(1, actionItems[0].GetProperty("athleteId").GetInt32());
        Assert.Equal("SetupRequired", actionItems[1].GetProperty("type").GetString());
        Assert.Equal(2, actionItems[1].GetProperty("athleteId").GetInt32());
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
    public async Task GetActionItems_WithTypeFilter_ReturnsPaginatedMatchingTasks()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync(
            "/api/coach-hub/action-items?type=AssessmentReview&page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, body.RootElement.GetProperty("pageSize").GetInt32());
        var item = body.RootElement.GetProperty("items")[0];
        Assert.Equal("AssessmentReview", item.GetProperty("type").GetString());
        Assert.Equal(1, item.GetProperty("athleteId").GetInt32());
    }

    [Fact]
    public async Task GetActionItems_WithAthleteToken_Returns403()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/action-items");

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
    [InlineData("/api/coach-hub/action-items")]
    [InlineData("/api/coach-hub/roster?filter=AwaitingAssessmentReview")]
    public async Task CoachHubEndpoints_WithRoleOnlyAdmin_Return200(string path)
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "admin@test.com", "Admin@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProgressReport_WithCoachToken_ReturnsReportAndKeepsSensitiveDataExcluded()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/athletes/1/progress-report?weeks=4");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(1, body.RootElement.GetProperty("athleteId").GetInt32());
        Assert.Equal(4, body.RootElement.GetProperty("weeks").GetInt32());
        Assert.Equal(4, body.RootElement.GetProperty("weeklyProgress").GetArrayLength());
        Assert.Equal(0, body.RootElement.GetProperty("coachNotes").GetArrayLength());
        Assert.Equal(0, body.RootElement.GetProperty("progressPhotos").GetArrayLength());
        var defaultCheckIn = body.RootElement.GetProperty("checkIns")[0];
        Assert.Equal(JsonValueKind.Null, defaultCheckIn.GetProperty("reviewNotes").ValueKind);
        Assert.Equal(JsonValueKind.Null, defaultCheckIn.GetProperty("reviewedAt").ValueKind);
        Assert.Equal(JsonValueKind.Null, body.RootElement.GetProperty("summary").GetProperty("weightChangeKg").ValueKind);

        var optedInResponse = await _client.GetAsync(
            "/api/coach-hub/athletes/1/progress-report?weeks=4&includeCoachNotes=true&includePhotos=true");
        Assert.Equal(HttpStatusCode.OK, optedInResponse.StatusCode);
        using var optedInBody = JsonDocument.Parse(await optedInResponse.Content.ReadAsStringAsync());
        var optedInCheckIn = optedInBody.RootElement.GetProperty("checkIns")[0];
        Assert.Equal("Private review note", optedInCheckIn.GetProperty("reviewNotes").GetString());
        Assert.NotEqual(JsonValueKind.Null, optedInCheckIn.GetProperty("reviewedAt").ValueKind);
        var photos = optedInBody.RootElement.GetProperty("progressPhotos");
        Assert.Equal(1, photos.GetArrayLength());
        Assert.Contains("/uploads/sample-progress.png", photos[0].GetProperty("url").GetString());
    }

    [Fact]
    public async Task DownloadProgressReport_WithCoachToken_ReturnsPdf()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/athletes/1/progress-report/pdf?weeks=4");
        var bytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.True(bytes.Length > 4);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task DownloadProgressReport_InArabic_WithPhotoOption_ReturnsPdf()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync(
            "/api/coach-hub/athletes/1/progress-report/pdf?weeks=4&language=ar&includePhotos=true&includeCoachNotes=true");
        var bytes = await response.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.True(bytes.Length > 4);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));

        if (Environment.GetEnvironmentVariable("PDF_SAMPLE_OUTPUT") is { Length: > 0 } outputPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllBytesAsync(outputPath, bytes);
        }
    }

    [Fact]
    public async Task GetProgressReport_WithAdminWithoutCoachProfile_Returns200()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "admin@test.com", "Admin@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/athletes/1/progress-report?weeks=8");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProgressReport_ForAnotherCoachsAthlete_Returns404()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "other-coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/athletes/1/progress-report?weeks=8");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProgressReport_WithAthleteToken_Returns403()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/athletes/1/progress-report?weeks=8");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProgressReport_WithUnsupportedRange_Returns400()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/coach-hub/athletes/1/progress-report?weeks=6");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
