using JokerNutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JokerNutrition.Data.Contexts.ClassMapping;

public class FoodMapping : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.ToTable("Foods");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.SeedKey).HasMaxLength(200);
        builder.HasIndex(f => f.SeedKey).IsUnique();
        builder.Property(f => f.Name).HasMaxLength(200).IsRequired();
        builder.Property(f => f.NameAr).HasMaxLength(200);
        builder.Property(f => f.Category).HasMaxLength(50);
        builder.Property(f => f.CaloriesPer100g).HasPrecision(8, 2);
        builder.Property(f => f.ProteinPer100g).HasPrecision(8, 2);
        builder.Property(f => f.CarbsPer100g).HasPrecision(8, 2);
        builder.Property(f => f.FatPer100g).HasPrecision(8, 2);
        builder.Property(f => f.FiberPer100g).HasPrecision(8, 2);
        builder.Property(f => f.SourceDocument).HasMaxLength(300);
        builder.Property(f => f.ContentStatus).HasDefaultValue(JokerNutrition.Data.Enums.ContentStatus.Published);
        builder.Property(f => f.ContentVersion).HasDefaultValue(1);
    }
}

public class RecipeMapping : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.SeedKey).HasMaxLength(200);
        builder.HasIndex(r => r.SeedKey).IsUnique();
        builder.Property(r => r.Name).HasMaxLength(200).IsRequired();
        builder.Property(r => r.NameAr).HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(1000);
        builder.Property(r => r.DescriptionAr).HasMaxLength(2000);
        builder.Property(r => r.UsageNotes).HasMaxLength(2000);
        builder.Property(r => r.UsageNotesAr).HasMaxLength(4000);
        builder.Property(r => r.Tags).HasMaxLength(1000);
        builder.Property(r => r.SourceDocument).HasMaxLength(300);
        builder.Property(r => r.ContentStatus).HasDefaultValue(JokerNutrition.Data.Enums.ContentStatus.Published);
        builder.Property(r => r.ContentVersion).HasDefaultValue(1);
        builder.Property(r => r.TotalCalories).HasPrecision(10, 2);
        builder.Property(r => r.TotalProtein).HasPrecision(8, 2);
        builder.Property(r => r.TotalCarbs).HasPrecision(8, 2);
        builder.Property(r => r.TotalFat).HasPrecision(8, 2);
        builder.Property(r => r.DeclaredCalories).HasPrecision(10, 2);
        builder.Property(r => r.DeclaredProtein).HasPrecision(8, 2);
        builder.Property(r => r.DeclaredCarbs).HasPrecision(8, 2);
        builder.Property(r => r.DeclaredFat).HasPrecision(8, 2);
        builder.HasOne(r => r.CreatedByAthlete).WithMany().HasForeignKey(r => r.CreatedByAthleteId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class RecipeIngredientMapping : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("RecipeIngredients");
        builder.HasKey(ri => ri.Id);
        builder.Property(ri => ri.QuantityGrams).HasPrecision(8, 2);
        builder.Property(ri => ri.DisplayQuantity).HasPrecision(8, 2);
        builder.Property(ri => ri.DisplayText).HasMaxLength(500);
        builder.Property(ri => ri.DisplayTextAr).HasMaxLength(1000);
        builder.Property(ri => ri.AlternativeGroupKey).HasMaxLength(100);
        builder.HasOne(ri => ri.Recipe).WithMany(r => r.Ingredients).HasForeignKey(ri => ri.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ri => ri.Food).WithMany(f => f.RecipeIngredients).HasForeignKey(ri => ri.FoodId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RecipeStepMapping : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("RecipeSteps");
        builder.HasKey(rs => rs.Id);
        builder.Property(rs => rs.Instruction).HasMaxLength(2000);
        builder.Property(rs => rs.InstructionAr).HasMaxLength(4000).IsRequired();
        builder.Property(rs => rs.MediaUrl).HasMaxLength(1000);
        builder.HasOne(rs => rs.Recipe).WithMany(r => r.Steps).HasForeignKey(rs => rs.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(rs => new { rs.RecipeId, rs.OrderIndex }).IsUnique();
    }
}

public class DailyDiaryMapping : IEntityTypeConfiguration<DailyDiary>
{
    public void Configure(EntityTypeBuilder<DailyDiary> builder)
    {
        builder.ToTable("DailyDiaries");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.TargetCalories).HasPrecision(10, 2);
        builder.Property(d => d.TargetProtein).HasPrecision(8, 2);
        builder.Property(d => d.TargetCarbs).HasPrecision(8, 2);
        builder.Property(d => d.TargetFat).HasPrecision(8, 2);
        builder.Property(d => d.WaterLitersConsumed).HasPrecision(5, 2);
        builder.Property(d => d.WaterLitersTarget).HasPrecision(5, 2);
        builder.HasOne(d => d.Athlete).WithMany(a => a.Diaries).HasForeignKey(d => d.AthleteId).OnDelete(DeleteBehavior.Cascade);
        // Ensure one diary per athlete per date
        builder.HasIndex(d => new { d.AthleteId, d.Date }).IsUnique();
    }
}

public class MealLogMapping : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.ToTable("MealLogs");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.QuantityGrams).HasPrecision(8, 2);
        builder.Property(m => m.Calories).HasPrecision(10, 2);
        builder.Property(m => m.Protein).HasPrecision(8, 2);
        builder.Property(m => m.Carbs).HasPrecision(8, 2);
        builder.Property(m => m.Fat).HasPrecision(8, 2);
        builder.Property(m => m.SnapshotName).HasMaxLength(300);
        builder.Property(m => m.SnapshotNameAr).HasMaxLength(300);
        builder.HasOne(m => m.DailyDiary).WithMany(d => d.MealLogs).HasForeignKey(m => m.DailyDiaryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Food).WithMany().HasForeignKey(m => m.FoodId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(m => m.Recipe).WithMany().HasForeignKey(m => m.RecipeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(m => m.NutritionPlanDiaryEntry).WithMany(e => e.MealLogs)
            .HasForeignKey(m => m.NutritionPlanDiaryEntryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(m => m.DailyDiaryId);
    }
}

public class NutritionPlanDiaryEntryMapping : IEntityTypeConfiguration<NutritionPlanDiaryEntry>
{
    public void Configure(EntityTypeBuilder<NutritionPlanDiaryEntry> builder)
    {
        builder.ToTable("NutritionPlanDiaryEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Servings).HasPrecision(6, 2);
        builder.Property(e => e.SelectionKey).HasMaxLength(1000).IsRequired();
        builder.HasOne(e => e.DailyDiary).WithMany(d => d.NutritionPlanEntries)
            .HasForeignKey(e => e.DailyDiaryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.NutritionPlanAssignment).WithMany(a => a.DiaryEntries)
            .HasForeignKey(e => e.NutritionPlanAssignmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.DailyDiaryId, e.NutritionPlanAssignmentId, e.NutritionMealBlockId })
            .IsUnique();
    }
}

public class MacroTargetMapping : IEntityTypeConfiguration<MacroTarget>
{
    public void Configure(EntityTypeBuilder<MacroTarget> builder)
    {
        builder.ToTable("MacroTargets");
        builder.HasKey(mt => mt.Id);
        builder.Property(mt => mt.TargetCalories).HasPrecision(10, 2);
        builder.Property(mt => mt.TargetProtein).HasPrecision(8, 2);
        builder.Property(mt => mt.TargetCarbs).HasPrecision(8, 2);
        builder.Property(mt => mt.TargetFat).HasPrecision(8, 2);
        builder.Property(mt => mt.WaterLitersTarget).HasPrecision(5, 2);
        builder.HasOne(mt => mt.Athlete).WithMany(a => a.MacroTargets).HasForeignKey(mt => mt.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(mt => mt.SetByCoach).WithMany().HasForeignKey(mt => mt.SetByCoachId).OnDelete(DeleteBehavior.Restrict);
        // Fast lookup for active targets by athlete
        builder.HasIndex(mt => new { mt.AthleteId, mt.IsActive });
    }
}

public class FavoriteFoodMapping : IEntityTypeConfiguration<FavoriteFood>
{
    public void Configure(EntityTypeBuilder<FavoriteFood> builder)
    {
        builder.ToTable("FavoriteFoods");
        builder.HasKey(ff => ff.Id);
        builder.HasOne(ff => ff.Athlete).WithMany().HasForeignKey(ff => ff.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ff => ff.Food).WithMany().HasForeignKey(ff => ff.FoodId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ff => new { ff.AthleteId, ff.FoodId }).IsUnique();
    }
}

public class FavoriteRecipeMapping : IEntityTypeConfiguration<FavoriteRecipe>
{
    public void Configure(EntityTypeBuilder<FavoriteRecipe> builder)
    {
        builder.ToTable("FavoriteRecipes");
        builder.HasKey(fr => fr.Id);
        builder.HasOne(fr => fr.Athlete).WithMany().HasForeignKey(fr => fr.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(fr => fr.Recipe).WithMany().HasForeignKey(fr => fr.RecipeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(fr => new { fr.AthleteId, fr.RecipeId }).IsUnique();
    }
}
