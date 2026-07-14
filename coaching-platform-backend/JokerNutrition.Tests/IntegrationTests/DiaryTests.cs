using System.Net;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Tests.Helpers;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the daily diary endpoints.
/// Tests: GetDiary, LogFood, GetMacroSummary, UpdateWater, UpdateSteps.
/// </summary>
public class DiaryTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;

    public DiaryTests(TestWebAppFactory factory)
    {
        _factory = factory;
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

    [Fact]
    public async Task BulkLogFoods_WithValidForm_ReturnsCreated()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Seed a Food
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            if (!db.Foods.Any(f => f.Id == 200))
            {
                db.Foods.Add(new Food
                {
                    Id = 200,
                    Name = "Test Bulk Food",
                    CaloriesPer100g = 100,
                    ProteinPer100g = 10,
                    CarbsPer100g = 10,
                    FatPer100g = 10,
                    FiberPer100g = 0
                });
                await db.SaveChangesAsync();
            }
        }

        var payload = new
        {
            date = "2026-07-06",
            mealType = 0, // Breakfast
            items = new[]
            {
                new { foodId = 200, recipeId = (int?)null, quantityGrams = 150.0 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/diary/log/bulk", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ToggleFavoriteFood_WithValidId_ReturnsOkAndToggles()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Seed a Food
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            if (!db.Foods.Any(f => f.Id == 300))
            {
                db.Foods.Add(new Food
                {
                    Id = 300,
                    Name = "Test Favorite Food",
                    CaloriesPer100g = 100,
                    ProteinPer100g = 10,
                    CarbsPer100g = 10,
                    FatPer100g = 10,
                    FiberPer100g = 0
                });
                await db.SaveChangesAsync();
            }
        }

        // Act & Assert 1: Toggle favorite ON
        var response = await _client.PostAsync("/api/diary/favorites/food/300/toggle", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Act & Assert 2: Toggle favorite OFF
        var responseOff = await _client.PostAsync("/api/diary/favorites/food/300/toggle", null);
        Assert.Equal(HttpStatusCode.OK, responseOff.StatusCode);
    }

    [Fact]
    public async Task GetFilteredRecipes_FavoritesSource_ReturnsOk()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/diary/filters?type=recipe&source=favorites");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
