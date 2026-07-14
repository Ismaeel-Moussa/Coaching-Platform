using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Seeding;

public sealed class CatalogManifest
{
    public string CatalogName { get; set; } = string.Empty;
    public string CatalogVersion { get; set; } = string.Empty;
    public Dictionary<string, CatalogFileManifest> Files { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<CatalogAssetManifest> Assets { get; set; } = new();
}

public sealed class CatalogFileManifest
{
    public string Path { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public int ExpectedCount { get; set; }
}

public sealed class CatalogAssetManifest
{
    public string Path { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
}

public sealed class CoachCatalogPackage
{
    public required CatalogManifest Manifest { get; init; }
    public required string ManifestChecksum { get; init; }
    public List<FoodSeedRecord> Foods { get; init; } = new();
    public List<ExerciseSeedRecord> Exercises { get; init; } = new();
    public List<RecipeSeedRecord> Recipes { get; init; } = new();
    public List<WorkoutTemplateSeedRecord> WorkoutTemplates { get; init; } = new();
    public List<NutritionPlanSeedRecord> NutritionTemplates { get; init; } = new();
    public List<SupplementCatalogSeedRecord> Supplements { get; init; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<CoachCatalogPackage> LoadAsync(string catalogDirectory, CancellationToken cancellationToken = default)
    {
        var root = Path.GetFullPath(catalogDirectory);
        var manifestPath = Path.Combine(root, "manifest.json");
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Catalog manifest.json was not found.", manifestPath);

        var manifestBytes = await File.ReadAllBytesAsync(manifestPath, cancellationToken);
        var manifest = JsonSerializer.Deserialize<CatalogManifest>(manifestBytes, JsonOptions)
            ?? throw new InvalidDataException("Catalog manifest is empty or invalid.");

        if (string.IsNullOrWhiteSpace(manifest.CatalogName) || string.IsNullOrWhiteSpace(manifest.CatalogVersion))
            throw new InvalidDataException("Catalog name and version are required.");

        string ResolveCatalogPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
                throw new InvalidDataException($"Catalog path '{relativePath}' is invalid.");

            var path = Path.GetFullPath(Path.Combine(root, relativePath));
            var relative = Path.GetRelativePath(root, path);
            if (relative.Equals("..", StringComparison.Ordinal) ||
                relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                throw new InvalidDataException($"Catalog path '{relativePath}' escapes the catalog directory.");
            return path;
        }

        async Task<List<T>> LoadSection<T>(string section)
        {
            if (!manifest.Files.TryGetValue(section, out var fileManifest))
                return new List<T>();

            var path = ResolveCatalogPath(fileManifest.Path);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Catalog section '{section}' was not found.", path);

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var actualHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (!actualHash.Equals(fileManifest.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Checksum mismatch for catalog section '{section}'.");

            var records = JsonSerializer.Deserialize<List<T>>(bytes, JsonOptions)
                ?? throw new InvalidDataException($"Catalog section '{section}' is invalid.");
            if (records.Count != fileManifest.ExpectedCount)
                throw new InvalidDataException($"Catalog section '{section}' expected {fileManifest.ExpectedCount} records but contains {records.Count}.");
            return records;
        }

        var duplicateAsset = manifest.Assets
            .GroupBy(asset => asset.Path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateAsset is not null)
            throw new InvalidDataException($"Catalog asset '{duplicateAsset.Key}' is listed more than once.");

        foreach (var asset in manifest.Assets)
        {
            var path = ResolveCatalogPath(asset.Path);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Catalog asset '{asset.Path}' was not found.", path);
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var actualHash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (!actualHash.Equals(asset.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Checksum mismatch for catalog asset '{asset.Path}'.");
        }

        return new CoachCatalogPackage
        {
            Manifest = manifest,
            ManifestChecksum = Convert.ToHexString(SHA256.HashData(manifestBytes)).ToLowerInvariant(),
            Foods = await LoadSection<FoodSeedRecord>("foods"),
            Exercises = await LoadSection<ExerciseSeedRecord>("exercises"),
            Recipes = await LoadSection<RecipeSeedRecord>("recipes"),
            WorkoutTemplates = await LoadSection<WorkoutTemplateSeedRecord>("workoutTemplates"),
            NutritionTemplates = await LoadSection<NutritionPlanSeedRecord>("nutritionTemplates"),
            Supplements = await LoadSection<SupplementCatalogSeedRecord>("supplements")
        };
    }
}

public abstract class CatalogSeedRecord
{
    public string SeedKey { get; set; } = string.Empty;
    public int ContentVersion { get; set; } = 1;
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Draft;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
}

public sealed class FoodSeedRecord : CatalogSeedRecord
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Category { get; set; }
    public FoodPreparationState PreparationState { get; set; }
    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }
    public decimal FiberPer100g { get; set; }
}

public sealed class ExerciseSeedRecord : CatalogSeedRecord
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }
    public MuscleGroup PrimaryMuscle { get; set; }
    public string? EquipmentRequired { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class RecipeSeedRecord : CatalogSeedRecord
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? UsageNotes { get; set; }
    public string? UsageNotesAr { get; set; }
    public RecipeCategory Category { get; set; } = RecipeCategory.Custom;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; } = 1;
    public string? Tags { get; set; }
    public string? ImageAssetPath { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public decimal? DeclaredCalories { get; set; }
    public decimal? DeclaredProtein { get; set; }
    public decimal? DeclaredCarbs { get; set; }
    public decimal? DeclaredFat { get; set; }
    public List<RecipeIngredientSeedRecord> Ingredients { get; set; } = new();
    public List<RecipeStepSeedRecord> Steps { get; set; } = new();
}

public sealed class RecipeIngredientSeedRecord
{
    public string FoodSeedKey { get; set; } = string.Empty;
    public decimal QuantityGrams { get; set; }
    public decimal? DisplayQuantity { get; set; }
    public IngredientUnit Unit { get; set; } = IngredientUnit.Gram;
    public FoodPreparationState MeasurementState { get; set; }
    public string? DisplayText { get; set; }
    public string? DisplayTextAr { get; set; }
    public bool IsOptional { get; set; }
    public string? AlternativeGroupKey { get; set; }
    public int OrderIndex { get; set; }
}

public sealed class RecipeStepSeedRecord
{
    public int OrderIndex { get; set; }
    public string? Instruction { get; set; }
    public string InstructionAr { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
}

public sealed class WorkoutTemplateSeedRecord : CatalogSeedRecord
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? Guidance { get; set; }
    public string? GuidanceAr { get; set; }
    public int? DailyStepTarget { get; set; }
    public List<WorkoutDaySeedRecord> Days { get; set; } = new();
}

public sealed class WorkoutDaySeedRecord
{
    public int DayNumber { get; set; }
    public string DayLabel { get; set; } = string.Empty;
    public string? DayLabelAr { get; set; }
    public bool IsRestDay { get; set; }
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }
    public string? CardioInstructions { get; set; }
    public string? CardioInstructionsAr { get; set; }
    public List<TemplateExerciseSeedRecord> Exercises { get; set; } = new();
}

public sealed class TemplateExerciseSeedRecord
{
    public string ExerciseSeedKey { get; set; } = string.Empty;
    public ExerciseSection Section { get; set; } = ExerciseSection.Main;
    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public string TargetReps { get; set; } = string.Empty;
    public int? RestSeconds { get; set; }
    public decimal? TargetRir { get; set; }
    public string? CoachNotes { get; set; }
    public string? CoachNotesAr { get; set; }
    public string? AlternativeGroupKey { get; set; }
}

public sealed class NutritionPlanSeedRecord : CatalogSeedRecord
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal TargetCalories { get; set; }
    public decimal MinimumProteinGrams { get; set; }
    public List<NutritionMealBlockSeedRecord> MealBlocks { get; set; } = new();
    public List<NutritionPlanRuleSeedRecord> Rules { get; set; } = new();
}

public sealed class NutritionMealBlockSeedRecord
{
    public int OrderIndex { get; set; }
    public MealType MealType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public decimal? TargetCalories { get; set; }
    public bool TrainingDayOnly { get; set; }
    public bool RestDayOnly { get; set; }
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }
    public List<NutritionMealOptionSeedRecord> Options { get; set; } = new();
}

public sealed class NutritionMealOptionSeedRecord
{
    public int OrderIndex { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public bool IsCompleteOption { get; set; } = true;
    public List<NutritionOptionItemSeedRecord> Items { get; set; } = new();
}

public sealed class NutritionOptionItemSeedRecord
{
    public int OrderIndex { get; set; }
    public string? FoodSeedKey { get; set; }
    public string? RecipeSeedKey { get; set; }
    public string? ItemName { get; set; }
    public string? ItemNameAr { get; set; }
    public decimal Quantity { get; set; }
    public IngredientUnit Unit { get; set; } = IngredientUnit.Gram;
    public FoodPreparationState MeasurementState { get; set; }
    public string? AlternativeGroupKey { get; set; }
}

public sealed class NutritionPlanRuleSeedRecord
{
    public int OrderIndex { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string TextAr { get; set; } = string.Empty;
}

public sealed class SupplementCatalogSeedRecord : CatalogSeedRecord
{
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public SupplementType Type { get; set; }
    public string? Education { get; set; }
    public string? EducationAr { get; set; }
    public string? SafetyWarning { get; set; }
    public string? SafetyWarningAr { get; set; }
    public bool RequiresClinicianApproval { get; set; }
}

public sealed record CatalogSectionResult(int Inserts, int Updates, int Unchanged);

public sealed class CatalogImportResult
{
    public string CatalogName { get; init; } = string.Empty;
    public string CatalogVersion { get; init; } = string.Empty;
    public bool DryRun { get; init; }
    public Dictionary<string, CatalogSectionResult> Sections { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}
