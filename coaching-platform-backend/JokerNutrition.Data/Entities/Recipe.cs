using JokerNutrition.Data.Enums;

namespace JokerNutrition.Data.Entities;

public class Recipe
{
    public int Id { get; set; }
    public string? SeedKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public string? DescriptionAr { get; set; }
    public string? UsageNotes { get; set; }
    public string? UsageNotesAr { get; set; }
    public RecipeCategory Category { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public bool IsJokerRecipe { get; set; } = false;
    public int? CreatedByAthleteId { get; set; }
    public Athlete? CreatedByAthlete { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ContentStatus ContentStatus { get; set; } = ContentStatus.Published;
    public int ContentVersion { get; set; } = 1;
    public string? SourceDocument { get; set; }
    public int? SourcePage { get; set; }
    public string? Tags { get; set; }

    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }

    public decimal? DeclaredCalories { get; set; }
    public decimal? DeclaredProtein { get; set; }
    public decimal? DeclaredCarbs { get; set; }
    public decimal? DeclaredFat { get; set; }

    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
}

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int FoodId { get; set; }
    public Food Food { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
    public decimal? DisplayQuantity { get; set; }
    public IngredientUnit Unit { get; set; } = IngredientUnit.Gram;
    public FoodPreparationState MeasurementState { get; set; } = FoodPreparationState.Unspecified;
    public string? DisplayText { get; set; }
    public string? DisplayTextAr { get; set; }
    public bool IsOptional { get; set; }
    public string? AlternativeGroupKey { get; set; }
    public int OrderIndex { get; set; }
}

public class RecipeStep
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int OrderIndex { get; set; }
    public string? Instruction { get; set; }
    public string InstructionAr { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
}
