using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the Invitation endpoints and workflows.
/// </summary>
public class InvitationTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public InvitationTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetInvitations_WithCoachToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/invitations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendInvitation_WithCoachToken_CreatesInvitationAndReturns201()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            email = "newathlete@test.com",
            role = "Athlete",
            expiryHours = 24
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/invitations", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task SendInvitation_WithAthleteToken_Returns403()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            email = "anothercoach@test.com",
            role = "Coach",
            expiryHours = 24
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/invitations", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ValidateToken_WithInvalidToken_Returns404()
    {
        // Act - invitation tokens are publicly validated during registration
        var response = await _client.GetAsync("/api/invitations/validate/nonexistent-token-value");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
