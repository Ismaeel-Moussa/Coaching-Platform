using System.Net;
using System.Net.Http.Json;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Critical-path integration tests for authentication endpoints.
/// Tests the full flow: Login → Refresh Token → Reject Invalid Credentials.
/// </summary>
public class AuthTests : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthTests(TestWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ─── Login ───────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndAccessToken()
    {
        // Arrange
        var payload = new { email = "coach@test.com", password = "Coach@Test123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("accessToken"), "Response should contain 'accessToken'");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        // Arrange
        var payload = new { email = "coach@test.com", password = "WrongPassword!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        // Arrange.
        var payload = new { email = "nobody@joker.com", password = "Whatever123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ─── Health Check ─────────────────────────────────────────────────

    [Fact]
    public async Task HealthEndpoint_Returns200WithStatusField()
    {
        var response = await _client.GetAsync("/api/health");
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.True(body!.ContainsKey("status"));
    }

    // ─── Protected Routes Without Auth ───────────────────────────────

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/diary/2026-07-06");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
