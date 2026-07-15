using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using JokerNutrition.Data.Enums;
using JokerNutrition.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JokerNutrition.Tests.IntegrationTests;

public class CatalogImporterTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task CatalogLoader_ValidatesSyntheticContentAndAssets()
    {
        await using var catalog = await CreateSyntheticCatalogAsync();
        var package = await CoachCatalogPackage.LoadAsync(catalog.DirectoryPath);

        Assert.Single(package.Exercises);
        Assert.Equal(2, package.Foods.Count);
        Assert.Single(package.Recipes);
        Assert.Single(package.WorkoutTemplates);
        Assert.Single(package.Supplements);
        Assert.Single(package.Manifest.Assets);
        Assert.Equal("assets/sample-recipe.webp", package.Recipes[0].ImageAssetPath);
    }

    [Fact]
    public async Task Importer_IsIdempotent_AndDoesNotCreateAthleteAssignments()
    {
        var options = new DbContextOptionsBuilder<JokerNutritionContext>()
            .UseInMemoryDatabase($"catalog-import-{Guid.NewGuid()}")
            .Options;
        await using var context = new JokerNutritionContext(options);
        var coachUser = new User
        {
            Email = "catalog-coach@example.com",
            UserName = "catalog-coach@example.com",
            FirstName = "Catalog",
            LastName = "Coach",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.Users.Add(coachUser);
        context.Coaches.Add(new Coach { User = coachUser, IsActive = true });
        await context.SaveChangesAsync();

        await using var catalog = await CreateSyntheticCatalogAsync();
        var package = await CoachCatalogPackage.LoadAsync(catalog.DirectoryPath);
        var importer = new CatalogImporter(context);
        var dryRun = await importer.ImportAsync(package, true, coachUser.Email, "test");
        Assert.Equal(1, dryRun.Sections["recipes"].Inserts);

        await importer.ImportAsync(package, false, coachUser.Email, "test");

        Assert.Single(await context.Exercises.Where(exercise => exercise.SeedKey != null).ToListAsync());
        var importedRecipe = Assert.Single(await context.Recipes
            .Include(recipe => recipe.Ingredients)
            .Where(recipe => recipe.SeedKey != null)
            .ToListAsync());
        Assert.Equal(2, importedRecipe.Ingredients.Count);
        Assert.Equal(100m, importedRecipe.TotalCalories);
        Assert.Equal(10m, importedRecipe.TotalProtein);
        Assert.Equal(20m, importedRecipe.TotalCarbs);
        Assert.Equal(5m, importedRecipe.TotalFat);
        Assert.Single(await context.TemplateExercises.ToListAsync());
        Assert.Single(await context.SupplementCatalogItems.ToListAsync());
        Assert.Empty(await context.SupplementSchedules.ToListAsync());
        Assert.Empty(await context.ClientPrograms.ToListAsync());

        var secondDryRun = await importer.ImportAsync(package, true, coachUser.Email, "test");
        Assert.All(secondDryRun.Sections.Values, section =>
        {
            Assert.Equal(0, section.Inserts);
            Assert.Equal(0, section.Updates);
        });
    }

    private static async Task<TemporaryCatalog> CreateSyntheticCatalogAsync()
    {
        var root = Path.Combine(Path.GetTempPath(), $"joker-catalog-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var manifest = new CatalogManifest
        {
            CatalogName = "synthetic-test-catalog",
            CatalogVersion = "1.0.0-test"
        };

        async Task WriteSectionAsync<T>(string section, string filename, List<T> records)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(records, JsonOptions);
            await File.WriteAllBytesAsync(Path.Combine(root, filename), bytes);
            manifest.Files[section] = new CatalogFileManifest
            {
                Path = filename,
                ExpectedCount = records.Count,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
            };
        }

        var exercise = new ExerciseSeedRecord
        {
            SeedKey = "test.exercise.press",
            ContentVersion = 1,
            ContentStatus = ContentStatus.Published,
            Name = "Test Press",
            PrimaryMuscle = MuscleGroup.Chest,
            IsActive = true
        };
        var defaultFood = new FoodSeedRecord
        {
            SeedKey = "test.food.default",
            ContentVersion = 1,
            ContentStatus = ContentStatus.Published,
            Name = "Default food",
            CaloriesPer100g = 100m,
            ProteinPer100g = 10m,
            CarbsPer100g = 20m,
            FatPer100g = 5m
        };
        var optionalAlternative = new FoodSeedRecord
        {
            SeedKey = "test.food.optional-alternative",
            ContentVersion = 1,
            ContentStatus = ContentStatus.Published,
            Name = "Optional alternative",
            CaloriesPer100g = 1000m,
            ProteinPer100g = 100m,
            CarbsPer100g = 100m,
            FatPer100g = 100m
        };
        var recipe = new RecipeSeedRecord
        {
            SeedKey = "test.recipe.meal",
            ContentVersion = 1,
            ContentStatus = ContentStatus.Draft,
            Name = "Test Meal",
            Servings = 1,
            ImageAssetPath = "assets/sample-recipe.webp",
            Ingredients =
            {
                new RecipeIngredientSeedRecord
                {
                    FoodSeedKey = defaultFood.SeedKey,
                    QuantityGrams = 100m,
                    AlternativeGroupKey = "food-choice",
                    OrderIndex = 1
                },
                new RecipeIngredientSeedRecord
                {
                    FoodSeedKey = optionalAlternative.SeedKey,
                    QuantityGrams = 100m,
                    IsOptional = true,
                    AlternativeGroupKey = "food-choice",
                    OrderIndex = 2
                }
            }
        };
        var workout = new WorkoutTemplateSeedRecord
        {
            SeedKey = "test.workout.template",
            ContentVersion = 1,
            ContentStatus = ContentStatus.InReview,
            Name = "Test Workout",
            Days =
            {
                new WorkoutDaySeedRecord
                {
                    DayNumber = 1,
                    DayLabel = "Day 1",
                    Exercises =
                    {
                        new TemplateExerciseSeedRecord
                        {
                            ExerciseSeedKey = exercise.SeedKey,
                            OrderIndex = 1,
                            TargetSets = 3,
                            TargetReps = "8-12"
                        }
                    }
                }
            }
        };
        var supplement = new SupplementCatalogSeedRecord
        {
            SeedKey = "test.supplement.item",
            ContentVersion = 1,
            ContentStatus = ContentStatus.InReview,
            Name = "Test Supplement",
            Type = SupplementType.Optional
        };

        await WriteSectionAsync("foods", "foods.json", new List<FoodSeedRecord> { defaultFood, optionalAlternative });
        await WriteSectionAsync("exercises", "exercises.json", new List<ExerciseSeedRecord> { exercise });
        await WriteSectionAsync("recipes", "recipes.json", new List<RecipeSeedRecord> { recipe });
        await WriteSectionAsync("workoutTemplates", "workout-templates.json", new List<WorkoutTemplateSeedRecord> { workout });
        await WriteSectionAsync("nutritionTemplates", "nutrition-templates.json", new List<NutritionPlanSeedRecord>());
        await WriteSectionAsync("supplements", "supplements.json", new List<SupplementCatalogSeedRecord> { supplement });

        var assetDirectory = Path.Combine(root, "assets");
        Directory.CreateDirectory(assetDirectory);
        var assetBytes = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50 };
        await File.WriteAllBytesAsync(Path.Combine(assetDirectory, "sample-recipe.webp"), assetBytes);
        manifest.Assets.Add(new CatalogAssetManifest
        {
            Path = "assets/sample-recipe.webp",
            ContentType = "image/webp",
            Sha256 = Convert.ToHexString(SHA256.HashData(assetBytes)).ToLowerInvariant()
        });

        await File.WriteAllBytesAsync(
            Path.Combine(root, "manifest.json"),
            JsonSerializer.SerializeToUtf8Bytes(manifest, JsonOptions));
        return new TemporaryCatalog(root);
    }

    private sealed class TemporaryCatalog(string directoryPath) : IAsyncDisposable
    {
        public string DirectoryPath { get; } = directoryPath;

        public ValueTask DisposeAsync()
        {
            if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, true);
            return ValueTask.CompletedTask;
        }
    }
}
