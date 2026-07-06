using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the Profile endpoints.
/// </summary>
public class ProfileTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public ProfileTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProfile_WithAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/profile");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_WithAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            firstName = "UpdatedAthlete",
            lastName = "Name",
            weightKg = 75.5,
            heightCm = 180.0,
            targetGoal = "Lean Bulk"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidConfirmPassword_ReturnsBadRequest()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        var payload = new
        {
            currentPassword = "Athlete@Test123!",
            newPassword = "NewSecretPassword123!",
            confirmPassword = "DifferentConfirmPassword!"
        };

        // Act
        var response = await _client.PutAsJsonAsync("/api/profile/change-password", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
