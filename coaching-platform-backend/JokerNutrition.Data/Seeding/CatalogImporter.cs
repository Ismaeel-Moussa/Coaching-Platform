using System.Text.Json;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace JokerNutrition.Data.Seeding;

public sealed class CatalogImporter(JokerNutritionContext context)
{
    private const long AdvisoryLockKey = 817263941L;

    public async Task<CatalogImportResult> ImportAsync(
        CoachCatalogPackage package,
        bool dryRun,
        string? coachEmail,
        string appliedBy,
        CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(package, coachEmail, cancellationToken);
        if (validation.Errors.Count > 0)
            throw new InvalidDataException("Catalog validation failed:\n- " + string.Join("\n- ", validation.Errors));

        var result = await BuildDiffAsync(package, dryRun, validation.Warnings, cancellationToken);
        if (dryRun)
            return result;

        var batch = new SeedImportBatch
        {
            CatalogName = package.Manifest.CatalogName,
            CatalogVersion = package.Manifest.CatalogVersion,
            ManifestChecksum = package.ManifestChecksum,
            Status = SeedImportStatus.Running,
            StartedAt = DateTime.UtcNow,
            AppliedBy = appliedBy
        };
        context.SeedImportBatches.Add(batch);
        await context.SaveChangesAsync(cancellationToken);

        IDbContextTransaction? transaction = null;
        try
        {
            if (context.Database.IsRelational())
            {
                transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                await context.Database.ExecuteSqlRawAsync(
                    $"SELECT pg_advisory_xact_lock({AdvisoryLockKey});", cancellationToken);
            }

            await ApplyFoodsAsync(package.Foods, cancellationToken);
            await ApplyExercisesAsync(package.Exercises, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            await ApplyRecipesAsync(package.Recipes, cancellationToken);
            await ApplyWorkoutTemplatesAsync(package.WorkoutTemplates, coachEmail, cancellationToken);
            await ApplyNutritionTemplatesAsync(package.NutritionTemplates, cancellationToken);
            await ApplySupplementsAsync(package.Supplements, cancellationToken);

            batch.Status = SeedImportStatus.Succeeded;
            batch.CompletedAt = DateTime.UtcNow;
            batch.SummaryJson = JsonSerializer.Serialize(result);
            await context.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            if (transaction is not null)
                await transaction.RollbackAsync(cancellationToken);

            context.ChangeTracker.Clear();
            var failedBatch = await context.SeedImportBatches.FindAsync([batch.Id], cancellationToken);
            if (failedBatch is not null)
            {
                failedBatch.Status = SeedImportStatus.Failed;
                failedBatch.CompletedAt = DateTime.UtcNow;
                failedBatch.Error = ex.Message.Length <= 4000 ? ex.Message : ex.Message[..4000];
                await context.SaveChangesAsync(cancellationToken);
            }

            throw;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private async Task<(List<string> Errors, List<string> Warnings)> ValidateAsync(
        CoachCatalogPackage package,
        string? coachEmail,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateRecords(package.Foods, "food", errors);
        ValidateRecords(package.Exercises, "exercise", errors);
        ValidateRecords(package.Recipes, "recipe", errors);
        ValidateRecords(package.WorkoutTemplates, "workout template", errors);
        ValidateRecords(package.NutritionTemplates, "nutrition template", errors);
        ValidateRecords(package.Supplements, "supplement", errors);

        var packageFoodKeys = package.Foods.Select(x => x.SeedKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var packageExerciseKeys = package.Exercises.Select(x => x.SeedKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var packageRecipeKeys = package.Recipes.Select(x => x.SeedKey).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var referencedFoodKeys = package.Recipes.SelectMany(r => r.Ingredients).Select(i => i.FoodSeedKey)
            .Concat(package.NutritionTemplates.SelectMany(t => t.MealBlocks)
                .SelectMany(b => b.Options).SelectMany(o => o.Items)
                .Where(i => !string.IsNullOrWhiteSpace(i.FoodSeedKey)).Select(i => i.FoodSeedKey!))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var referencedExerciseKeys = package.WorkoutTemplates.SelectMany(t => t.Days)
            .SelectMany(d => d.Exercises).Select(e => e.ExerciseSeedKey)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var referencedRecipeKeys = package.NutritionTemplates.SelectMany(t => t.MealBlocks)
            .SelectMany(b => b.Options).SelectMany(o => o.Items)
            .Where(i => !string.IsNullOrWhiteSpace(i.RecipeSeedKey)).Select(i => i.RecipeSeedKey!)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var existingFoodKeys = await context.Foods.Where(f => f.SeedKey != null && referencedFoodKeys.Contains(f.SeedKey))
            .Select(f => f.SeedKey!).ToListAsync(cancellationToken);
        var existingExerciseKeys = await context.Exercises.Where(e => e.SeedKey != null && referencedExerciseKeys.Contains(e.SeedKey))
            .Select(e => e.SeedKey!).ToListAsync(cancellationToken);
        var existingRecipeKeys = await context.Recipes.Where(r => r.SeedKey != null && referencedRecipeKeys.Contains(r.SeedKey))
            .Select(r => r.SeedKey!).ToListAsync(cancellationToken);

        var availableFoods = packageFoodKeys.Concat(existingFoodKeys).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var availableExercises = packageExerciseKeys.Concat(existingExerciseKeys).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var availableRecipes = packageRecipeKeys.Concat(existingRecipeKeys).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var key in referencedFoodKeys.Where(key => !availableFoods.Contains(key)))
            errors.Add($"Food reference '{key}' cannot be resolved.");
        foreach (var key in referencedExerciseKeys.Where(key => !availableExercises.Contains(key)))
            errors.Add($"Exercise reference '{key}' cannot be resolved.");
        foreach (var key in referencedRecipeKeys.Where(key => !availableRecipes.Contains(key)))
            errors.Add($"Recipe reference '{key}' cannot be resolved.");

        foreach (var recipe in package.Recipes)
        {
            if (recipe.Servings < 1) errors.Add($"Recipe '{recipe.SeedKey}' must have at least one serving.");
            if (recipe.Ingredients.GroupBy(i => i.OrderIndex).Any(g => g.Count() > 1))
                errors.Add($"Recipe '{recipe.SeedKey}' has duplicate ingredient order indexes.");
            if (recipe.Steps.GroupBy(s => s.OrderIndex).Any(g => g.Count() > 1))
                errors.Add($"Recipe '{recipe.SeedKey}' has duplicate step order indexes.");
            if (recipe.Ingredients.Any(i => i.QuantityGrams < 0))
                errors.Add($"Recipe '{recipe.SeedKey}' contains a negative ingredient quantity.");
            if (recipe.ContentStatus == ContentStatus.Published && (recipe.Ingredients.Count == 0 || recipe.Steps.Count == 0))
                errors.Add($"Published recipe '{recipe.SeedKey}' requires ingredients and preparation steps.");
            if (recipe.ContentStatus != ContentStatus.Published && (recipe.Ingredients.Count == 0 || recipe.Steps.Count == 0))
                warnings.Add($"Draft recipe '{recipe.SeedKey}' is incomplete and will remain hidden from published-only views.");
        }

        foreach (var template in package.WorkoutTemplates)
        {
            if (template.Days.GroupBy(d => d.DayNumber).Any(g => g.Count() > 1))
                errors.Add($"Workout template '{template.SeedKey}' has duplicate day numbers.");
            if (template.Days.Any(d => d.DayNumber is < 1 or > 7))
                errors.Add($"Workout template '{template.SeedKey}' contains a day outside 1-7.");
            foreach (var day in template.Days)
            {
                if (day.Exercises.GroupBy(e => e.OrderIndex).Any(g => g.Count() > 1))
                    errors.Add($"Workout template '{template.SeedKey}' day {day.DayNumber} has duplicate exercise order indexes.");
                if (day.IsRestDay && day.Exercises.Count > 0)
                    errors.Add($"Workout template '{template.SeedKey}' day {day.DayNumber} is a rest day but contains exercises.");
            }
        }

        foreach (var nutrition in package.NutritionTemplates)
        {
            if (nutrition.MealBlocks.GroupBy(b => b.OrderIndex).Any(g => g.Count() > 1))
                errors.Add($"Nutrition template '{nutrition.SeedKey}' has duplicate meal-block order indexes.");
            var blockCalories = nutrition.MealBlocks.Where(b => b.TargetCalories.HasValue).Sum(b => b.TargetCalories!.Value);
            if (blockCalories > 0 && blockCalories != nutrition.TargetCalories)
                warnings.Add($"Nutrition template '{nutrition.SeedKey}' meal blocks total {blockCalories} kcal but headline target is {nutrition.TargetCalories} kcal.");
            foreach (var item in nutrition.MealBlocks.SelectMany(b => b.Options).SelectMany(o => o.Items))
            {
                var referenceCount = new[] { item.FoodSeedKey, item.RecipeSeedKey, item.ItemName, item.ItemNameAr }
                    .Count(value => !string.IsNullOrWhiteSpace(value));
                if (referenceCount == 0)
                    errors.Add($"Nutrition template '{nutrition.SeedKey}' contains an option item without a food, recipe, or display name.");
            }
        }

        if (package.WorkoutTemplates.Count > 0)
        {
            if (string.IsNullOrWhiteSpace(coachEmail))
                errors.Add("A coach email is required when importing workout templates.");
            else
            {
                var coachExists = await context.Coaches.AnyAsync(c => c.User.Email == coachEmail, cancellationToken);
                if (!coachExists) errors.Add($"Coach '{coachEmail}' was not found.");
            }
        }

        await RejectDowngradesAsync(package, errors, cancellationToken);
        return (errors, warnings);
    }

    private static void ValidateRecords<T>(IEnumerable<T> records, string label, List<string> errors) where T : CatalogSeedRecord
    {
        var materialized = records.ToList();
        foreach (var record in materialized)
        {
            if (string.IsNullOrWhiteSpace(record.SeedKey)) errors.Add($"A {label} is missing SeedKey.");
            if (record.ContentVersion < 1) errors.Add($"{label} '{record.SeedKey}' has an invalid content version.");
        }
        foreach (var duplicate in materialized.GroupBy(r => r.SeedKey, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            errors.Add($"Duplicate {label} SeedKey '{duplicate.Key}'.");
    }

    private async Task RejectDowngradesAsync(CoachCatalogPackage package, List<string> errors, CancellationToken cancellationToken)
    {
        async Task Check<TRecord, TEntity>(
            IReadOnlyCollection<TRecord> records,
            IQueryable<TEntity> query,
            Func<TEntity, string?> key,
            Func<TEntity, int> version,
            string label) where TRecord : CatalogSeedRecord
        {
            if (records.Count == 0) return;
            var entities = await query.ToListAsync(cancellationToken);
            var existing = entities.Where(e => key(e) is not null).ToDictionary(e => key(e)!, version, StringComparer.OrdinalIgnoreCase);
            foreach (var record in records.Where(r => existing.TryGetValue(r.SeedKey, out var current) && r.ContentVersion < current))
                errors.Add($"{label} '{record.SeedKey}' would downgrade content version {existing[record.SeedKey]} to {record.ContentVersion}.");
        }

        await Check(package.Foods, context.Foods, x => x.SeedKey, x => x.ContentVersion, "Food");
        await Check(package.Exercises, context.Exercises, x => x.SeedKey, x => x.ContentVersion, "Exercise");
        await Check(package.Recipes, context.Recipes, x => x.SeedKey, x => x.ContentVersion, "Recipe");
        await Check(package.WorkoutTemplates, context.WorkoutTemplates, x => x.SeedKey, x => x.ContentVersion, "Workout template");
        await Check(package.NutritionTemplates, context.NutritionPlanTemplates, x => x.SeedKey, x => x.ContentVersion, "Nutrition template");
        await Check(package.Supplements, context.SupplementCatalogItems, x => x.SeedKey, x => x.ContentVersion, "Supplement");
    }

    private async Task<CatalogImportResult> BuildDiffAsync(
        CoachCatalogPackage package,
        bool dryRun,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        async Task<CatalogSectionResult> Count<TRecord, TEntity>(
            IReadOnlyCollection<TRecord> records,
            IQueryable<TEntity> query,
            Func<TEntity, string?> key,
            Func<TEntity, int> version) where TRecord : CatalogSeedRecord
        {
            var entities = await query.ToListAsync(cancellationToken);
            var existing = entities.Where(e => key(e) is not null).ToDictionary(e => key(e)!, version, StringComparer.OrdinalIgnoreCase);
            var inserts = records.Count(r => !existing.ContainsKey(r.SeedKey));
            var updates = records.Count(r => existing.TryGetValue(r.SeedKey, out var current) && r.ContentVersion > current);
            return new CatalogSectionResult(inserts, updates, records.Count - inserts - updates);
        }

        return new CatalogImportResult
        {
            CatalogName = package.Manifest.CatalogName,
            CatalogVersion = package.Manifest.CatalogVersion,
            DryRun = dryRun,
            Warnings = warnings,
            Sections = new Dictionary<string, CatalogSectionResult>
            {
                ["foods"] = await Count(package.Foods, context.Foods, x => x.SeedKey, x => x.ContentVersion),
                ["exercises"] = await Count(package.Exercises, context.Exercises, x => x.SeedKey, x => x.ContentVersion),
                ["recipes"] = await Count(package.Recipes, context.Recipes, x => x.SeedKey, x => x.ContentVersion),
                ["workoutTemplates"] = await Count(package.WorkoutTemplates, context.WorkoutTemplates, x => x.SeedKey, x => x.ContentVersion),
                ["nutritionTemplates"] = await Count(package.NutritionTemplates, context.NutritionPlanTemplates, x => x.SeedKey, x => x.ContentVersion),
                ["supplements"] = await Count(package.Supplements, context.SupplementCatalogItems, x => x.SeedKey, x => x.ContentVersion)
            }
        };
    }

    private async Task ApplyFoodsAsync(IReadOnlyCollection<FoodSeedRecord> records, CancellationToken cancellationToken)
    {
        var keys = records.Select(r => r.SeedKey).ToList();
        var existing = await context.Foods.Where(f => f.SeedKey != null && keys.Contains(f.SeedKey))
            .ToDictionaryAsync(f => f.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var record in records)
        {
            if (!existing.TryGetValue(record.SeedKey, out var entity))
            {
                entity = new Food { SeedKey = record.SeedKey, IsCustom = false };
                context.Foods.Add(entity);
            }
            else if (record.ContentVersion <= entity.ContentVersion) continue;

            entity.Name = record.Name;
            entity.NameAr = record.NameAr;
            entity.Category = record.Category;
            entity.PreparationState = record.PreparationState;
            entity.CaloriesPer100g = record.CaloriesPer100g;
            entity.ProteinPer100g = record.ProteinPer100g;
            entity.CarbsPer100g = record.CarbsPer100g;
            entity.FatPer100g = record.FatPer100g;
            entity.FiberPer100g = record.FiberPer100g;
            entity.ContentStatus = record.ContentStatus;
            entity.ContentVersion = record.ContentVersion;
            entity.SourceDocument = record.SourceDocument;
            entity.SourcePage = record.SourcePage;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task ApplyExercisesAsync(IReadOnlyCollection<ExerciseSeedRecord> records, CancellationToken cancellationToken)
    {
        var keys = records.Select(r => r.SeedKey).ToList();
        var existing = await context.Exercises.Where(e => e.SeedKey != null && keys.Contains(e.SeedKey))
            .ToDictionaryAsync(e => e.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var record in records)
        {
            if (!existing.TryGetValue(record.SeedKey, out var entity))
            {
                entity = new Exercise { SeedKey = record.SeedKey, CreatedAt = DateTime.UtcNow };
                context.Exercises.Add(entity);
            }
            else if (record.ContentVersion <= entity.ContentVersion) continue;

            entity.Name = record.Name;
            entity.NameAr = record.NameAr;
            entity.Instructions = record.Instructions;
            entity.InstructionsAr = record.InstructionsAr;
            entity.PrimaryMuscle = record.PrimaryMuscle;
            entity.EquipmentRequired = record.EquipmentRequired;
            entity.VideoUrl = record.VideoUrl;
            entity.YouTubeVideoId = ExtractYouTubeId(record.VideoUrl);
            entity.IsActive = record.IsActive;
            entity.ContentStatus = record.ContentStatus;
            entity.ContentVersion = record.ContentVersion;
            entity.SourceDocument = record.SourceDocument;
            entity.SourcePage = record.SourcePage;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task ApplyRecipesAsync(IReadOnlyCollection<RecipeSeedRecord> records, CancellationToken cancellationToken)
    {
        var keys = records.Select(r => r.SeedKey).ToList();
        var existing = await context.Recipes
            .Include(r => r.Ingredients).Include(r => r.Steps)
            .Where(r => r.SeedKey != null && keys.Contains(r.SeedKey))
            .ToDictionaryAsync(r => r.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var foodKeys = records.SelectMany(r => r.Ingredients).Select(i => i.FoodSeedKey).Distinct().ToList();
        var foods = await context.Foods.Where(f => f.SeedKey != null && foodKeys.Contains(f.SeedKey))
            .ToDictionaryAsync(f => f.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var record in records)
        {
            if (!existing.TryGetValue(record.SeedKey, out var entity))
            {
                entity = new Recipe { SeedKey = record.SeedKey, CreatedAt = DateTime.UtcNow, IsJokerRecipe = true };
                context.Recipes.Add(entity);
            }
            else
            {
                if (record.ContentVersion <= entity.ContentVersion) continue;
                context.RecipeIngredients.RemoveRange(entity.Ingredients);
                context.RecipeSteps.RemoveRange(entity.Steps);
                entity.Ingredients = new List<RecipeIngredient>();
                entity.Steps = new List<RecipeStep>();
            }

            entity.Name = record.Name;
            entity.NameAr = record.NameAr;
            entity.Description = record.Description;
            entity.DescriptionAr = record.DescriptionAr;
            entity.UsageNotes = record.UsageNotes;
            entity.UsageNotesAr = record.UsageNotesAr;
            entity.Category = record.Category;
            entity.PrepTimeMinutes = record.PrepTimeMinutes;
            entity.CookTimeMinutes = record.CookTimeMinutes;
            entity.Servings = record.Servings;
            entity.Tags = record.Tags;
            entity.ImageUrl = record.ImageUrl;
            entity.VideoUrl = record.VideoUrl;
            entity.DeclaredCalories = record.DeclaredCalories;
            entity.DeclaredProtein = record.DeclaredProtein;
            entity.DeclaredCarbs = record.DeclaredCarbs;
            entity.DeclaredFat = record.DeclaredFat;
            entity.ContentStatus = record.ContentStatus;
            entity.ContentVersion = record.ContentVersion;
            entity.SourceDocument = record.SourceDocument;
            entity.SourcePage = record.SourcePage;
            entity.UpdatedAt = DateTime.UtcNow;

            entity.Ingredients = record.Ingredients.OrderBy(i => i.OrderIndex).Select(item => new RecipeIngredient
            {
                FoodId = foods[item.FoodSeedKey].Id,
                Food = foods[item.FoodSeedKey],
                QuantityGrams = item.QuantityGrams,
                DisplayQuantity = item.DisplayQuantity,
                Unit = item.Unit,
                MeasurementState = item.MeasurementState,
                DisplayText = item.DisplayText,
                DisplayTextAr = item.DisplayTextAr,
                IsOptional = item.IsOptional,
                AlternativeGroupKey = item.AlternativeGroupKey,
                OrderIndex = item.OrderIndex
            }).ToList();
            entity.Steps = record.Steps.OrderBy(s => s.OrderIndex).Select(step => new RecipeStep
            {
                OrderIndex = step.OrderIndex,
                Instruction = step.Instruction,
                InstructionAr = step.InstructionAr,
                MediaUrl = step.MediaUrl
            }).ToList();

            entity.TotalCalories = entity.Ingredients.Sum(i => i.Food.CaloriesPer100g * i.QuantityGrams / 100m);
            entity.TotalProtein = entity.Ingredients.Sum(i => i.Food.ProteinPer100g * i.QuantityGrams / 100m);
            entity.TotalCarbs = entity.Ingredients.Sum(i => i.Food.CarbsPer100g * i.QuantityGrams / 100m);
            entity.TotalFat = entity.Ingredients.Sum(i => i.Food.FatPer100g * i.QuantityGrams / 100m);
        }
    }

    private async Task ApplyWorkoutTemplatesAsync(
        IReadOnlyCollection<WorkoutTemplateSeedRecord> records,
        string? coachEmail,
        CancellationToken cancellationToken)
    {
        if (records.Count == 0) return;
        var coach = await context.Coaches.Include(c => c.User)
            .SingleAsync(c => c.User.Email == coachEmail, cancellationToken);
        var keys = records.Select(r => r.SeedKey).ToList();
        var existing = await context.WorkoutTemplates.Include(t => t.Days).ThenInclude(d => d.Exercises)
            .Where(t => t.SeedKey != null && keys.Contains(t.SeedKey))
            .ToDictionaryAsync(t => t.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var exerciseKeys = records.SelectMany(t => t.Days).SelectMany(d => d.Exercises)
            .Select(e => e.ExerciseSeedKey).Distinct().ToList();
        var exercises = await context.Exercises.Where(e => e.SeedKey != null && exerciseKeys.Contains(e.SeedKey))
            .ToDictionaryAsync(e => e.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var record in records)
        {
            if (!existing.TryGetValue(record.SeedKey, out var entity))
            {
                entity = new WorkoutTemplate { SeedKey = record.SeedKey, CreatedAt = DateTime.UtcNow, CreatedByCoachId = coach.Id };
                context.WorkoutTemplates.Add(entity);
            }
            else
            {
                if (record.ContentVersion <= entity.ContentVersion) continue;
                context.WorkoutTemplateDays.RemoveRange(entity.Days);
                entity.Days = new List<WorkoutTemplateDay>();
            }

            entity.Name = record.Name;
            entity.NameAr = record.NameAr;
            entity.Description = record.Description;
            entity.DescriptionAr = record.DescriptionAr;
            entity.Guidance = record.Guidance;
            entity.GuidanceAr = record.GuidanceAr;
            entity.DailyStepTarget = record.DailyStepTarget;
            entity.ContentStatus = record.ContentStatus;
            entity.ContentVersion = record.ContentVersion;
            entity.SourceDocument = record.SourceDocument;
            entity.SourcePage = record.SourcePage;
            entity.IsActive = record.ContentStatus != ContentStatus.Archived;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Days = record.Days.OrderBy(d => d.DayNumber).Select(day => new WorkoutTemplateDay
            {
                DayNumber = day.DayNumber,
                DayLabel = day.DayLabel,
                DayLabelAr = day.DayLabelAr,
                IsRestDay = day.IsRestDay,
                Instructions = day.Instructions,
                InstructionsAr = day.InstructionsAr,
                CardioInstructions = day.CardioInstructions,
                CardioInstructionsAr = day.CardioInstructionsAr,
                Exercises = day.Exercises.OrderBy(e => e.OrderIndex).Select(item => new TemplateExercise
                {
                    ExerciseId = exercises[item.ExerciseSeedKey].Id,
                    Exercise = exercises[item.ExerciseSeedKey],
                    Section = item.Section,
                    OrderIndex = item.OrderIndex,
                    TargetSets = item.TargetSets,
                    TargetReps = item.TargetReps,
                    RestSeconds = item.RestSeconds,
                    TargetRir = item.TargetRir,
                    CoachNotes = item.CoachNotes,
                    CoachNotesAr = item.CoachNotesAr,
                    AlternativeGroupKey = item.AlternativeGroupKey
                }).ToList()
            }).ToList();
        }
    }

    private async Task ApplyNutritionTemplatesAsync(IReadOnlyCollection<NutritionPlanSeedRecord> records, CancellationToken cancellationToken)
    {
        var keys = records.Select(r => r.SeedKey).ToList();
        var existing = await context.NutritionPlanTemplates
            .Include(t => t.Rules)
            .Include(t => t.MealBlocks).ThenInclude(b => b.Options).ThenInclude(o => o.Items)
            .Where(t => keys.Contains(t.SeedKey))
            .ToDictionaryAsync(t => t.SeedKey, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var foodKeys = records.SelectMany(t => t.MealBlocks).SelectMany(b => b.Options).SelectMany(o => o.Items)
            .Where(i => i.FoodSeedKey is not null).Select(i => i.FoodSeedKey!).Distinct().ToList();
        var recipeKeys = records.SelectMany(t => t.MealBlocks).SelectMany(b => b.Options).SelectMany(o => o.Items)
            .Where(i => i.RecipeSeedKey is not null).Select(i => i.RecipeSeedKey!).Distinct().ToList();
        var foods = await context.Foods.Where(f => f.SeedKey != null && foodKeys.Contains(f.SeedKey))
            .ToDictionaryAsync(f => f.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var recipes = await context.Recipes.Where(r => r.SeedKey != null && recipeKeys.Contains(r.SeedKey))
            .ToDictionaryAsync(r => r.SeedKey!, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var record in records)
        {
            if (!existing.TryGetValue(record.SeedKey, out var entity))
            {
                entity = new NutritionPlanTemplate { SeedKey = record.SeedKey, CreatedAt = DateTime.UtcNow };
                context.NutritionPlanTemplates.Add(entity);
            }
            else
            {
                if (record.ContentVersion <= entity.ContentVersion) continue;
                context.NutritionMealBlocks.RemoveRange(entity.MealBlocks);
                context.NutritionPlanRules.RemoveRange(entity.Rules);
                entity.MealBlocks = new List<NutritionMealBlock>();
                entity.Rules = new List<NutritionPlanRule>();
            }

            entity.Name = record.Name;
            entity.NameAr = record.NameAr;
            entity.Description = record.Description;
            entity.DescriptionAr = record.DescriptionAr;
            entity.TargetCalories = record.TargetCalories;
            entity.MinimumProteinGrams = record.MinimumProteinGrams;
            entity.ContentStatus = record.ContentStatus;
            entity.ContentVersion = record.ContentVersion;
            entity.SourceDocument = record.SourceDocument;
            entity.SourcePage = record.SourcePage;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Rules = record.Rules.OrderBy(r => r.OrderIndex).Select(rule => new NutritionPlanRule
            {
                OrderIndex = rule.OrderIndex,
                RuleType = rule.RuleType,
                Text = rule.Text,
                TextAr = rule.TextAr
            }).ToList();
            entity.MealBlocks = record.MealBlocks.OrderBy(b => b.OrderIndex).Select(block => new NutritionMealBlock
            {
                OrderIndex = block.OrderIndex,
                MealType = block.MealType,
                Label = block.Label,
                LabelAr = block.LabelAr,
                TargetCalories = block.TargetCalories,
                TrainingDayOnly = block.TrainingDayOnly,
                RestDayOnly = block.RestDayOnly,
                Instructions = block.Instructions,
                InstructionsAr = block.InstructionsAr,
                Options = block.Options.OrderBy(o => o.OrderIndex).Select(option => new NutritionMealOption
                {
                    OrderIndex = option.OrderIndex,
                    Label = option.Label,
                    LabelAr = option.LabelAr,
                    IsCompleteOption = option.IsCompleteOption,
                    Items = option.Items.OrderBy(i => i.OrderIndex).Select(item => new NutritionOptionItem
                    {
                        OrderIndex = item.OrderIndex,
                        FoodId = item.FoodSeedKey is null ? null : foods[item.FoodSeedKey].Id,
                        Food = item.FoodSeedKey is null ? null : foods[item.FoodSeedKey],
                        RecipeId = item.RecipeSeedKey is null ? null : recipes[item.RecipeSeedKey].Id,
                        Recipe = item.RecipeSeedKey is null ? null : recipes[item.RecipeSeedKey],
                        ItemName = item.ItemName,
                        ItemNameAr = item.ItemNameAr,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        MeasurementState = item.MeasurementState,
                        AlternativeGroupKey = item.AlternativeGroupKey
                    }).ToList()
                }).ToList()
            }).ToList();
        }
    }

    private async Task ApplySupplementsAsync(IReadOnlyCollection<SupplementCatalogSeedRecord> records, CancellationToken cancellationToken)
    {
        var keys = records.Select(r => r.SeedKey).ToList();
        var existing = await context.SupplementCatalogItems.Where(s => keys.Contains(s.SeedKey))
            .ToDictionaryAsync(s => s.SeedKey, StringComparer.OrdinalIgnoreCase, cancellationToken);
        foreach (var record in records)
        {
            if (!existing.TryGetValue(record.SeedKey, out var entity))
            {
                entity = new SupplementCatalogItem { SeedKey = record.SeedKey, CreatedAt = DateTime.UtcNow };
                context.SupplementCatalogItems.Add(entity);
            }
            else if (record.ContentVersion <= entity.ContentVersion) continue;

            entity.Name = record.Name;
            entity.NameAr = record.NameAr;
            entity.Type = record.Type;
            entity.Education = record.Education;
            entity.EducationAr = record.EducationAr;
            entity.SafetyWarning = record.SafetyWarning;
            entity.SafetyWarningAr = record.SafetyWarningAr;
            entity.RequiresClinicianApproval = record.RequiresClinicianApproval;
            entity.ContentStatus = record.ContentStatus;
            entity.ContentVersion = record.ContentVersion;
            entity.SourceDocument = record.SourceDocument;
            entity.SourcePage = record.SourcePage;
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static string? ExtractYouTubeId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri)) return null;
        if (uri.Host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
            return uri.AbsolutePath.Trim('/').Split('/')[0];
        if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
        {
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
        return null;
    }
}
