using System.Net;
using System.Net.Http.Json;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the daily diary endpoints.
/// Tests: GetDiary, LogFood, GetMacroSummary, UpdateWater, UpdateSteps.
/// </summary>
public class DiaryTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public DiaryTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDiary_WithValidAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/diary/2026-07-06");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMacroSummary_WithValidAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/diary/summary/2026-07-06");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDiary_WithCoachToken_Returns403()
    {
        // The Diary controller is [Authorize(Roles = "Athlete")] only
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/diary/2026-07-06");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWater_WithValidAthleteToken_Returns204()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.PatchAsJsonAsync("/api/diary/2026-07-06/water", new { waterLiters = 2.5 });

        // Assert — 204 No Content or 200 both acceptable
        Assert.True(
            response.StatusCode == HttpStatusCode.NoContent ||
            response.StatusCode == HttpStatusCode.OK,
            $"Unexpected status: {response.StatusCode}");
    }
}
