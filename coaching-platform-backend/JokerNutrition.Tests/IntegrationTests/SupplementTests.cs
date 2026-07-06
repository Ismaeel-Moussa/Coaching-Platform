using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the Supplements endpoints.
/// </summary>
public class SupplementTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public SupplementTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSchedule_WithAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/supplements");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AssignSupplement_WithCoachToken_Returns201()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            athleteId = 1, // Seeded athlete ID
            name = "Creatine Monohydrate",
            type = 0, // SupplementType enum value (e.g. Health, Performance, etc.)
            dosage = "5g",
            notes = "Take daily post-workout with water"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/supplements/schedule", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AssignSupplement_WithAthleteToken_Returns403()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            athleteId = 1,
            name = "Vitamin D3",
            type = 0,
            dosage = "5000 IU",
            notes = "With breakfast"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/supplements/schedule", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
