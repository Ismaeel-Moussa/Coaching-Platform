using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class SeedImportBatch
{
    public long Id { get; set; }
    public string CatalogName { get; set; } = string.Empty;
    public string CatalogVersion { get; set; } = string.Empty;
    public string ManifestChecksum { get; set; } = string.Empty;
    public SeedImportStatus Status { get; set; } = SeedImportStatus.Running;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? AppliedBy { get; set; }
    public string? SummaryJson { get; set; }
    public string? Error { get; set; }
}

public class NutritionPlanTemplate
{
    public int Id { get; set; }
    public string SeedKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public decimal TargetCalories { get; set; }
    public decimal MinimumProteinGrams { get; set; }
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Draft;
    public int ContentVersion { get; set; } = 1;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public bool IsManuallyEdited { get; set; }
    public int? LastEditedByUserId { get; set; }
    public DateTime? LastEditedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<NutritionMealBlock> MealBlocks { get; set; } = new List<NutritionMealBlock>();
    public ICollection<NutritionPlanRule> Rules { get; set; } = new List<NutritionPlanRule>();
    public ICollection<NutritionPlanAssignment> Assignments { get; set; } = new List<NutritionPlanAssignment>();
}

public class NutritionMealBlock
{
    public int Id { get; set; }
    public int NutritionPlanTemplateId { get; set; }
    public NutritionPlanTemplate Template { get; set; } = null!;
    public int OrderIndex { get; set; }
    public MealType MealType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public decimal? TargetCalories { get; set; }
    public bool TrainingDayOnly { get; set; }
    public bool RestDayOnly { get; set; }
    public string? Instructions { get; set; }
    public string? InstructionsAr { get; set; }

    public ICollection<NutritionMealOption> Options { get; set; } = new List<NutritionMealOption>();
}

public class NutritionMealOption
{
    public int Id { get; set; }
    public int NutritionMealBlockId { get; set; }
    public NutritionMealBlock MealBlock { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? LabelAr { get; set; }
    public bool IsCompleteOption { get; set; } = true;

    public ICollection<NutritionOptionItem> Items { get; set; } = new List<NutritionOptionItem>();
}

public class NutritionOptionItem
{
    public int Id { get; set; }
    public int NutritionMealOptionId { get; set; }
    public NutritionMealOption Option { get; set; } = null!;
    public int OrderIndex { get; set; }
    public int? FoodId { get; set; }
    public Food? Food { get; set; }
    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public string? ItemName { get; set; }
    public string? ItemNameAr { get; set; }
    public decimal Quantity { get; set; }
    public IngredientUnit Unit { get; set; } = IngredientUnit.Gram;
    public FoodPreparationState MeasurementState { get; set; } = FoodPreparationState.Unspecified;
    public string? AlternativeGroupKey { get; set; }
}

public class NutritionPlanRule
{
    public int Id { get; set; }
    public int NutritionPlanTemplateId { get; set; }
    public NutritionPlanTemplate Template { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string? Text { get; set; }
    public string TextAr { get; set; } = string.Empty;
}

public class NutritionPlanAssignment
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int NutritionPlanTemplateId { get; set; }
    public NutritionPlanTemplate Template { get; set; } = null!;
    public int? AssignedByCoachId { get; set; }
    public Coach? AssignedByCoach { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string SnapshotJson { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime AssignedAt { get; set; }

    public ICollection<NutritionPlanDiaryEntry> DiaryEntries { get; set; } = new List<NutritionPlanDiaryEntry>();
}
