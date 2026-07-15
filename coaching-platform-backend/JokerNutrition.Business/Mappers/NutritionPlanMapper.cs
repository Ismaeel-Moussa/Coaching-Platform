using System.Text.Json;
using System.Text.Json.Serialization;
using JokerNutrition.Business.DTOs.NutritionPlans;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class NutritionPlanMapper
{
    public static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static NutritionPlanSummaryDto MapSummary(NutritionPlanTemplate template)
    {
        var (trainingCalories, restCalories, hasConditionalBlocks) = CalculateDayCalories(template);
        return new()
        {
        Id = template.Id,
        SeedKey = template.SeedKey,
        Name = template.Name,
        NameAr = template.NameAr,
        TargetCalories = template.TargetCalories,
        MinimumProteinGrams = template.MinimumProteinGrams,
        ContentStatus = template.ContentStatus,
        ContentVersion = template.ContentVersion,
        MealBlockCount = template.MealBlocks.Count,
        MealBlockCalories = hasConditionalBlocks ? Math.Max(trainingCalories, restCalories) : trainingCalories,
        TrainingDayCalories = trainingCalories,
        RestDayCalories = restCalories,
        ActiveAssignmentCount = template.Assignments.Count(assignment => assignment.IsActive),
        IsManuallyEdited = template.IsManuallyEdited,
        UpdatedAt = template.UpdatedAt
        };
    }

    public static NutritionPlanDto MapFull(NutritionPlanTemplate template)
    {
        var summary = MapSummary(template);
        return new NutritionPlanDto
        {
            Id = summary.Id,
            SeedKey = summary.SeedKey,
            Name = summary.Name,
            NameAr = summary.NameAr,
            Description = template.Description,
            DescriptionAr = template.DescriptionAr,
            TargetCalories = summary.TargetCalories,
            MinimumProteinGrams = summary.MinimumProteinGrams,
            ContentStatus = summary.ContentStatus,
            ContentVersion = summary.ContentVersion,
            MealBlockCount = summary.MealBlockCount,
            MealBlockCalories = summary.MealBlockCalories,
            TrainingDayCalories = summary.TrainingDayCalories,
            RestDayCalories = summary.RestDayCalories,
            ActiveAssignmentCount = summary.ActiveAssignmentCount,
            IsManuallyEdited = summary.IsManuallyEdited,
            UpdatedAt = summary.UpdatedAt,
            SourceDocument = template.SourceDocument,
            SourcePage = template.SourcePage,
            MealBlocks = template.MealBlocks.OrderBy(block => block.OrderIndex).Select(block => new NutritionMealBlockDto
            {
                Id = block.Id,
                OrderIndex = block.OrderIndex,
                MealType = block.MealType,
                Label = block.Label,
                LabelAr = block.LabelAr,
                TargetCalories = block.TargetCalories,
                TrainingDayOnly = block.TrainingDayOnly,
                RestDayOnly = block.RestDayOnly,
                Instructions = block.Instructions,
                InstructionsAr = block.InstructionsAr,
                Options = block.Options.OrderBy(option => option.OrderIndex).Select(option => new NutritionMealOptionDto
                {
                    Id = option.Id,
                    OrderIndex = option.OrderIndex,
                    Label = option.Label,
                    LabelAr = option.LabelAr,
                    IsCompleteOption = option.IsCompleteOption,
                    Items = option.Items.OrderBy(item => item.OrderIndex).Select(item => new NutritionOptionItemDto
                    {
                        Id = item.Id,
                        OrderIndex = item.OrderIndex,
                        FoodId = item.FoodId,
                        FoodName = item.Food?.Name,
                        FoodNameAr = item.Food?.NameAr,
                        RecipeId = item.RecipeId,
                        RecipeName = item.Recipe?.Name,
                        RecipeNameAr = item.Recipe?.NameAr,
                        ItemName = item.ItemName,
                        ItemNameAr = item.ItemNameAr,
                        Quantity = item.Quantity,
                        Unit = item.Unit,
                        MeasurementState = item.MeasurementState,
                        AlternativeGroupKey = item.AlternativeGroupKey,
                        CatalogContentVersion = item.Food?.ContentVersion ?? item.Recipe?.ContentVersion,
                        CaloriesPer100Grams = item.Food?.CaloriesPer100g,
                        ProteinPer100Grams = item.Food?.ProteinPer100g,
                        CarbsPer100Grams = item.Food?.CarbsPer100g,
                        FatPer100Grams = item.Food?.FatPer100g,
                        RecipeTotalWeightGrams = item.Recipe?.Ingredients.Sum(ingredient => ingredient.QuantityGrams),
                        RecipeTotalCalories = item.Recipe?.TotalCalories,
                        RecipeTotalProtein = item.Recipe?.TotalProtein,
                        RecipeTotalCarbs = item.Recipe?.TotalCarbs,
                        RecipeTotalFat = item.Recipe?.TotalFat
                    }).ToList()
                }).ToList()
            }).ToList(),
            Rules = template.Rules.OrderBy(rule => rule.OrderIndex).Select(rule => new NutritionPlanRuleDto
            {
                Id = rule.Id,
                OrderIndex = rule.OrderIndex,
                RuleType = rule.RuleType,
                Text = rule.Text,
                TextAr = rule.TextAr
            }).ToList()
        };
    }

    public static NutritionPlanAssignmentDto MapAssignment(NutritionPlanAssignment assignment, NutritionPlanDto plan) => new()
    {
        Id = assignment.Id,
        AthleteId = assignment.AthleteId,
        TemplateId = assignment.NutritionPlanTemplateId,
        TemplateName = plan.Name,
        TemplateNameAr = plan.NameAr,
        StartDate = assignment.StartDate,
        EndDate = assignment.EndDate,
        IsActive = assignment.IsActive,
        Notes = assignment.Notes,
        AssignedAt = assignment.AssignedAt,
        Plan = plan
    };

    private static (decimal TrainingDay, decimal RestDay, bool HasConditionalBlocks) CalculateDayCalories(
        NutritionPlanTemplate template)
    {
        var shared = template.MealBlocks
            .Where(block => !block.TrainingDayOnly && !block.RestDayOnly)
            .Sum(block => block.TargetCalories ?? 0);
        var training = shared + template.MealBlocks
            .Where(block => block.TrainingDayOnly && !block.RestDayOnly)
            .Sum(block => block.TargetCalories ?? 0);
        var rest = shared + template.MealBlocks
            .Where(block => block.RestDayOnly && !block.TrainingDayOnly)
            .Sum(block => block.TargetCalories ?? 0);
        var hasConditional = template.MealBlocks.Any(block => block.TrainingDayOnly || block.RestDayOnly);
        return (training, rest, hasConditional);
    }
}
