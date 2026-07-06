using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

/// <summary>
/// Integration tests verifying the Recipes endpoints.
/// </summary>
public class RecipeTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;

    public RecipeTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRecipes_WithAthleteToken_Returns200()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Act
        var response = await _client.GetAsync("/api/recipes");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_WithAthleteToken_Returns201()
    {
        // Arrange
        var token = await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
        AuthHelpers.SetBearerToken(_client, token);

        // Seed a Food to use as an ingredient
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            if (!db.Foods.Any(f => f.Id == 100))
            {
                db.Foods.Add(new Food
                {
                    Id = 100,
                    Name = "Eegs",
                    CaloriesPer100g = 143,
                    ProteinPer100g = 13,
                    CarbsPer100g = 1.1m,
                    FatPer100g = 9.5m,
                    FiberPer100g = 0
                });
                await db.SaveChangesAsync();
            }
        }

        var payload = new
        {
            name = "Boiled Eggs",
            prepTimeMinutes = 5,
            cookTimeMinutes = 10,
            servings = 2,
            ingredients = new List<object>
            {
                new { foodId = 100, quantityGrams = 100, state = 0 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/recipes", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
