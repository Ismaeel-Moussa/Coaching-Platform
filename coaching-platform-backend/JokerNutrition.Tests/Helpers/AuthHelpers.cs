using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace JokerNutrition.Tests.Helpers;

/// <summary>
/// Helper utilities for obtaining JWT tokens in integration tests.
/// </summary>
public static class AuthHelpers
{
    /// <summary>
    /// Posts to /api/auth/login and returns the accessToken string.
    /// Throws if login fails.
    /// </summary>
    public static async Task<string> GetAccessTokenAsync(
        HttpClient client,
        string email,
        string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>()
            ?? throw new InvalidOperationException("Login response was null.");

        return body.AccessToken;
    }

    /// <summary>
    /// Attaches a Bearer token to every subsequent request on the provided HttpClient.
    /// </summary>
    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    // ─── Private DTOs ────────────────────────────────────────────────
    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}
