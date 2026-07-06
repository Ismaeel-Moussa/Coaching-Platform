using System.Net;
using System.Net.Http.Json;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests for workout endpoints.
/// Tests: GetTodaysWorkout, GetProgram, LogSet, CompleteWorkout, GetHistory.
/// </summary>
public class WorkoutTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public WorkoutTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTodaysWorkout_WithAthleteToken_Returns200OrNoContent()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/workouts/today");

        // Assert — 200 with data or 204 if no program assigned yet
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NoContent,
            $"Unexpected status: {response.StatusCode}");
    }

    [Fact]
    public async Task GetWorkoutHistory_WithAthleteToken_Returns200()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/workouts/history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WorkoutEndpoints_WithCoachToken_Returns403()
    {
        // Workout log endpoints are Athlete-only
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var response = await _client.GetAsync("/api/workouts/today");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task LogSet_WithInvalidPayload_Returns400()
    {
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Missing required fields
        var response = await _client.PostAsJsonAsync("/api/workouts/log-set", new { });

        // 400 Bad Request because exerciseId / weight / reps are required
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
