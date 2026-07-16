using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using JokerNutrition.Business.DTOs.NutritionPlans;
using JokerNutrition.Business.DTOs.Diary;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

public class NutritionPlanTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private static readonly SemaphoreSlim TokenLock = new(1, 1);
    private static string? _coachToken;
    private static string? _athleteToken;

    public NutritionPlanTests(TestWebAppFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    [Fact]
    public async Task Coach_CanValidatePublishAssign_AndAthleteReceivesStableSnapshot()
    {
        await AuthenticateCoachAsync();

        var invalidPlan = BuildPlan("Integration nutrition plan", 2200, 1900);
        var createResponse = await _client.PostAsJsonAsync("/api/nutrition-plans", invalidPlan);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(created);

        var validation = await _client.GetFromJsonAsync<NutritionPlanValidationDto>(
            $"/api/nutrition-plans/{created.Id}/validation", _jsonOptions);
        Assert.NotNull(validation);
        Assert.False(validation.IsValidForPublish);
        Assert.Contains(validation.Issues, issue => issue.Code == "calorie_mismatch");

        var reviewResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/status", new { status = 1, expectedContentVersion = created.ContentVersion });
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(reviewed);

        var rejectedPublish = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/status", new { status = 2, expectedContentVersion = reviewed.ContentVersion });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, rejectedPublish.StatusCode);
        Assert.Contains("Meal blocks total", await rejectedPublish.Content.ReadAsStringAsync());

        var validPlan = BuildPlan("Integration nutrition plan", 1900, 1900, reviewed.ContentVersion);
        var updateResponse = await _client.PutAsJsonAsync($"/api/nutrition-plans/{created.Id}", validPlan);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(updated);

        var publishResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/status", new { status = 2, expectedContentVersion = updated.ContentVersion });
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        var published = await publishResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(published);

        var assignResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/assign",
            new { athleteIds = new[] { 1 }, notes = "Follow this plan for the next week." });
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        var editedPlan = BuildPlan("Changed after assignment", 1900, 1900, published.ContentVersion);
        var editResponse = await _client.PutAsJsonAsync($"/api/nutrition-plans/{created.Id}", editedPlan);
        Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);

        await AuthenticateAthleteAsync();
        var assignment = await _client.GetFromJsonAsync<NutritionPlanAssignmentDto>(
            "/api/nutrition-plans/me/current", _jsonOptions);

        Assert.NotNull(assignment);
        Assert.Equal(created.Id, assignment.TemplateId);
        Assert.Equal("Integration nutrition plan", assignment.TemplateName);
        Assert.Equal("Integration nutrition plan", assignment.Plan.Name);
        Assert.Equal("Follow this plan for the next week.", assignment.Notes);
    }

    [Fact]
    public async Task Athlete_CanLogAssignedPlanOption_OnlyOncePerBlockAndDate()
    {
        const int foodId = 910;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            if (!await db.Foods.AnyAsync(food => food.Id == foodId))
            {
                db.Foods.Add(new Food
                {
                    Id = foodId,
                    Name = "Plan diary food",
                    NameAr = "طعام الخطة",
                    ContentStatus = ContentStatus.Published,
                    CaloriesPer100g = 100,
                    ProteinPer100g = 20,
                    CarbsPer100g = 5,
                    FatPer100g = 2
                });
                await db.SaveChangesAsync();
            }
        }

        await AuthenticateCoachAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/api/nutrition-plans", BuildLoggablePlan(foodId));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(created);
        var reviewResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/status",
            new { status = 1, expectedContentVersion = created.ContentVersion });
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(reviewed);
        var publishResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/status",
            new { status = 2, expectedContentVersion = reviewed.ContentVersion });
        var published = await publishResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(published);
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{published.Id}/assign", new { athleteIds = new[] { 1 } })).StatusCode);

        // The assignment snapshot must keep its original nutrients even if the
        // live food catalog changes later.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            var food = await db.Foods.SingleAsync(item => item.Id == foodId);
            food.CaloriesPer100g = 500;
            food.ProteinPer100g = 1;
            await db.SaveChangesAsync();
        }

        await AuthenticateAthleteAsync();
        var assignment = await _client.GetFromJsonAsync<NutritionPlanAssignmentDto>(
            "/api/nutrition-plans/me/current", _jsonOptions);
        Assert.NotNull(assignment);
        var block = Assert.Single(assignment.Plan.MealBlocks);
        var option = Assert.Single(block.Options);
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var payload = new
        {
            assignmentId = assignment.Id,
            mealBlockId = block.Id,
            mealOptionId = option.Id,
            date = date.ToString("yyyy-MM-dd"),
            mealType = 0,
            servings = 2
        };

        var logResponse = await _client.PostAsJsonAsync("/api/diary/log/nutrition-plan", payload);
        Assert.Equal(HttpStatusCode.Created, logResponse.StatusCode);
        var entry = await logResponse.Content.ReadFromJsonAsync<NutritionPlanDiaryEntryDto>(_jsonOptions);
        Assert.NotNull(entry);
        var mealLog = Assert.Single(entry.MealLogs);
        Assert.Equal(300, mealLog.QuantityGrams);
        Assert.Equal(300, mealLog.Calories);

        var duplicateResponse = await _client.PostAsJsonAsync("/api/diary/log/nutrition-plan", payload);
        Assert.Equal(HttpStatusCode.Created, duplicateResponse.StatusCode);

        var completed = await _client.GetFromJsonAsync<List<NutritionPlanDiaryEntryDto>>(
            $"/api/diary/nutrition-plan/{assignment.Id}/{date:yyyy-MM-dd}", _jsonOptions);
        Assert.NotNull(completed);
        Assert.Single(completed);

        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await _client.DeleteAsync($"/api/diary/log/{mealLog.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,
            (await _client.DeleteAsync($"/api/diary/nutrition-plan/{entry.Id}")).StatusCode);
        completed = await _client.GetFromJsonAsync<List<NutritionPlanDiaryEntryDto>>(
            $"/api/diary/nutrition-plan/{assignment.Id}/{date:yyyy-MM-dd}", _jsonOptions);
        Assert.NotNull(completed);
        Assert.Empty(completed);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            var storedAssignment = await db.NutritionPlanAssignments.SingleAsync(item => item.Id == assignment.Id);
            storedAssignment.IsActive = false;
            storedAssignment.EndDate = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.UnprocessableEntity,
            (await _client.PostAsJsonAsync("/api/diary/log/nutrition-plan", payload)).StatusCode);
    }

    [Fact]
    public async Task ConditionalTrainingAndRestPlans_ValidateEachDaySeparately()
    {
        await AuthenticateCoachAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/nutrition-plans", BuildConditionalPlan());
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var plan = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(plan);

        var validation = await _client.GetFromJsonAsync<NutritionPlanValidationDto>(
            $"/api/nutrition-plans/{plan.Id}/validation", _jsonOptions);

        Assert.NotNull(validation);
        Assert.True(validation.IsValidForPublish);
        Assert.Equal(1900, validation.TrainingDayCalories);
        Assert.Equal(1900, validation.RestDayCalories);
    }

    [Fact]
    public async Task FutureAssignment_IsRejectedBeforeReplacingCurrentPlan()
    {
        await AuthenticateCoachAsync();
        var currentPlan = await CreatePublishedPlanAsync($"Current plan {Guid.NewGuid():N}");
        Assert.Equal(HttpStatusCode.OK, (await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{currentPlan.Id}/assign", new { athleteIds = new[] { 1 } })).StatusCode);
        var futurePlan = await CreatePublishedPlanAsync($"Future plan {Guid.NewGuid():N}");

        var response = await _client.PostAsJsonAsync($"/api/nutrition-plans/{futurePlan.Id}/assign", new
        {
            athleteIds = new[] { 1 },
            startDate = DateTime.UtcNow.AddDays(1)
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Future nutrition-plan assignments", await response.Content.ReadAsStringAsync());

        await AuthenticateAthleteAsync();
        var current = await _client.GetFromJsonAsync<NutritionPlanAssignmentDto>(
            "/api/nutrition-plans/me/current", _jsonOptions);
        Assert.NotNull(current);
        Assert.Equal(currentPlan.Id, current.TemplateId);
    }

    [Fact]
    public async Task InvalidNestedEnum_IsRejectedByModelValidation()
    {
        await AuthenticateCoachAsync();
        var payload = JsonSerializer.SerializeToNode(BuildPlan("Invalid enum", 1900, 1900))!.AsObject();
        payload["mealBlocks"]![0]!["mealType"] = 999;

        var response = await _client.PostAsJsonAsync("/api/nutrition-plans", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task StaleEditorSave_ReturnsConflict()
    {
        await AuthenticateCoachAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/api/nutrition-plans", BuildPlan("Concurrency plan", 1900, 1900));
        var created = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(created);

        var firstUpdate = await _client.PutAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}",
            BuildPlan("First editor", 1900, 1900, created.ContentVersion));
        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);

        var staleUpdate = await _client.PutAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}",
            BuildPlan("Stale editor", 1900, 1900, created.ContentVersion));

        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
    }

    [Fact]
    public async Task StatusChange_InvalidatesAnAlreadyOpenEditor()
    {
        await AuthenticateCoachAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/api/nutrition-plans", BuildPlan("Status concurrency plan", 1900, 1900));
        var created = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(created);

        var reviewResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}/status",
            new { status = 1, expectedContentVersion = created.ContentVersion });
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);

        var staleEditorSave = await _client.PutAsJsonAsync(
            $"/api/nutrition-plans/{created.Id}",
            BuildPlan("Stale after status change", 1900, 1900, created.ContentVersion));

        Assert.Equal(HttpStatusCode.Conflict, staleEditorSave.StatusCode);
    }

    [Fact]
    public async Task ContentVersion_IsARealEfConcurrencyToken()
    {
        await AuthenticateCoachAsync();
        var createResponse = await _client.PostAsJsonAsync(
            "/api/nutrition-plans", BuildPlan("EF concurrency plan", 1900, 1900));
        var created = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(created);

        using var scopeOne = _factory.Services.CreateScope();
        using var scopeTwo = _factory.Services.CreateScope();
        var dbOne = scopeOne.ServiceProvider.GetRequiredService<JokerNutritionContext>();
        var dbTwo = scopeTwo.ServiceProvider.GetRequiredService<JokerNutritionContext>();
        var first = await dbOne.NutritionPlanTemplates.SingleAsync(plan => plan.Id == created.Id);
        var second = await dbTwo.NutritionPlanTemplates.SingleAsync(plan => plan.Id == created.Id);
        first.Name = "First concurrent save";
        first.ContentVersion += 1;
        second.Name = "Second concurrent save";
        second.ContentVersion += 1;

        await dbOne.SaveChangesAsync();

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => dbTwo.SaveChangesAsync());
    }

    [Fact]
    public async Task FoodSearch_FindsArabicNameAndReturnsIt()
    {
        var uniqueArabicName = $"طعام-{Guid.NewGuid():N}";
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<JokerNutritionContext>();
            db.Foods.Add(new Food
            {
                Name = "Arabic searchable food",
                NameAr = uniqueArabicName,
                CaloriesPer100g = 100,
                ProteinPer100g = 10,
                CarbsPer100g = 10,
                FatPer100g = 2,
            });
            await db.SaveChangesAsync();
        }

        await AuthenticateAthleteAsync();
        var response = await _client.GetAsync($"/api/foods?search={Uri.EscapeDataString(uniqueArabicName)}&pageSize=10");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = JsonNode.Parse(body)!.AsObject();
        Assert.Contains(json["items"]!.AsArray(), item => item!["nameAr"]!.GetValue<string>() == uniqueArabicName);
    }

    [Fact]
    public async Task Athlete_CannotAccessCoachNutritionPlanEditorApi()
    {
        await AuthenticateAthleteAsync();

        var response = await _client.GetAsync("/api/nutrition-plans");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task AuthenticateCoachAsync()
    {
        if (_coachToken is null)
        {
            await TokenLock.WaitAsync();
            try
            {
                _coachToken ??= await AuthHelpers.GetAccessTokenAsync(_client, "coach@test.com", "Coach@Test123!");
            }
            finally
            {
                TokenLock.Release();
            }
        }
        AuthHelpers.SetBearerToken(_client, _coachToken);
    }

    private async Task AuthenticateAthleteAsync()
    {
        if (_athleteToken is null)
        {
            await TokenLock.WaitAsync();
            try
            {
                _athleteToken ??= await AuthHelpers.GetAccessTokenAsync(_client, "athlete@test.com", "Athlete@Test123!");
            }
            finally
            {
                TokenLock.Release();
            }
        }
        AuthHelpers.SetBearerToken(_client, _athleteToken);
    }

    private async Task<NutritionPlanDto> CreatePublishedPlanAsync(string name)
    {
        var createResponse = await _client.PostAsJsonAsync("/api/nutrition-plans", BuildPlan(name, 1900, 1900));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var plan = await createResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(plan);
        var reviewResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{plan.Id}/status", new { status = 1, expectedContentVersion = plan.ContentVersion });
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        var reviewed = await reviewResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(reviewed);
        var publishResponse = await _client.PostAsJsonAsync(
            $"/api/nutrition-plans/{plan.Id}/status", new { status = 2, expectedContentVersion = reviewed.ContentVersion });
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);
        var published = await publishResponse.Content.ReadFromJsonAsync<NutritionPlanDto>(_jsonOptions);
        Assert.NotNull(published);
        return published;
    }

    private static object BuildPlan(string name, decimal targetCalories, decimal mealCalories, int? expectedContentVersion = null) => new
    {
        name,
        description = "Integration test plan",
        targetCalories,
        minimumProteinGrams = 150,
        expectedContentVersion,
        mealBlocks = new[]
        {
            new
            {
                mealType = 0,
                label = "Daily meals",
                targetCalories = mealCalories,
                trainingDayOnly = false,
                restDayOnly = false,
                instructions = "Choose one option.",
                options = new[]
                {
                    new
                    {
                        label = "Option one",
                        isCompleteOption = true,
                        items = new[]
                        {
                            new
                            {
                                itemName = "Balanced meal",
                                quantity = 1,
                                unit = 2,
                                measurementState = 0
                            }
                        }
                    }
                }
            }
        },
        rules = new[]
        {
            new { ruleType = "general", text = "Drink water." }
        }
    };

    private static object BuildConditionalPlan() => new
    {
        name = "Conditional plan",
        targetCalories = 1900,
        minimumProteinGrams = 150,
        mealBlocks = new[]
        {
            BuildMealBlock("Shared meals", 1000, false, false),
            BuildMealBlock("Training meal", 900, true, false),
            BuildMealBlock("Rest meal", 900, false, true),
        },
        rules = Array.Empty<object>()
    };

    private static object BuildLoggablePlan(int foodId) => new
    {
        name = $"Loggable plan {Guid.NewGuid():N}",
        targetCalories = 300,
        minimumProteinGrams = 60,
        mealBlocks = new[]
        {
            new
            {
                mealType = 0,
                label = "Breakfast",
                targetCalories = 300,
                trainingDayOnly = false,
                restDayOnly = false,
                options = new[]
                {
                    new
                    {
                        label = "Oats option",
                        isCompleteOption = true,
                        items = new[]
                        {
                            new
                            {
                                foodId,
                                quantity = 150,
                                unit = 0,
                                measurementState = 0
                            }
                        }
                    }
                }
            }
        },
        rules = Array.Empty<object>()
    };

    private static object BuildMealBlock(string label, decimal calories, bool trainingOnly, bool restOnly) => new
    {
        mealType = 0,
        label,
        targetCalories = calories,
        trainingDayOnly = trainingOnly,
        restDayOnly = restOnly,
        options = new[]
        {
            new
            {
                label = "Option",
                isCompleteOption = true,
                items = new[]
                {
                    new { itemName = "Meal", quantity = 1, unit = 2, measurementState = 0 }
                }
            }
        }
    };
}
