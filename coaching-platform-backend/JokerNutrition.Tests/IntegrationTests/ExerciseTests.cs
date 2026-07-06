using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the Exercises endpoints.
/// </summary>
public class ExerciseTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public ExerciseTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetExercises_WithAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/exercises");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_WithCoachToken_Returns201()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            name = "Incline Dumbbell Press",
            primaryMuscle = "Chest",
            equipmentRequired = "Dumbbells, Incline Bench",
            instructions = "Press the dumbbells up from chest level while lying on an incline bench."
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_WithAthleteToken_Returns403()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            name = "Hammer Curls",
            primaryMuscle = "Biceps",
            equipmentRequired = "Dumbbells"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
