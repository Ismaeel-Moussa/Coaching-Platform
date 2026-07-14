using JokerNutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JokerNutrition.Data.Contexts.ClassMapping;

public class SeedImportBatchMapping : IEntityTypeConfiguration<SeedImportBatch>
{
    public void Configure(EntityTypeBuilder<SeedImportBatch> builder)
    {
        builder.ToTable("SeedImportBatches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.CatalogName).HasMaxLength(200).IsRequired();
        builder.Property(b => b.CatalogVersion).HasMaxLength(100).IsRequired();
        builder.Property(b => b.ManifestChecksum).HasMaxLength(128).IsRequired();
        builder.Property(b => b.AppliedBy).HasMaxLength(300);
        builder.Property(b => b.SummaryJson).HasColumnType("jsonb");
        builder.Property(b => b.Error).HasMaxLength(4000);
        builder.HasIndex(b => new { b.CatalogName, b.CatalogVersion, b.ManifestChecksum });
    }
}

public class NutritionPlanTemplateMapping : IEntityTypeConfiguration<NutritionPlanTemplate>
{
    public void Configure(EntityTypeBuilder<NutritionPlanTemplate> builder)
    {
        builder.ToTable("NutritionPlanTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.SeedKey).HasMaxLength(200).IsRequired();
        builder.HasIndex(t => t.SeedKey).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.NameAr).HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(2000);
        builder.Property(t => t.DescriptionAr).HasMaxLength(4000);
        builder.Property(t => t.TargetCalories).HasPrecision(10, 2);
        builder.Property(t => t.MinimumProteinGrams).HasPrecision(8, 2);
        builder.Property(t => t.SourceDocument).HasMaxLength(300);
        builder.Property(t => t.ContentVersion).HasDefaultValue(1);
    }
}

public class NutritionMealBlockMapping : IEntityTypeConfiguration<NutritionMealBlock>
{
    public void Configure(EntityTypeBuilder<NutritionMealBlock> builder)
    {
        builder.ToTable("NutritionMealBlocks");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Label).HasMaxLength(200).IsRequired();
        builder.Property(b => b.LabelAr).HasMaxLength(200);
        builder.Property(b => b.TargetCalories).HasPrecision(10, 2);
        builder.Property(b => b.Instructions).HasMaxLength(2000);
        builder.Property(b => b.InstructionsAr).HasMaxLength(4000);
        builder.HasOne(b => b.Template).WithMany(t => t.MealBlocks).HasForeignKey(b => b.NutritionPlanTemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(b => new { b.NutritionPlanTemplateId, b.OrderIndex }).IsUnique();
    }
}

public class NutritionMealOptionMapping : IEntityTypeConfiguration<NutritionMealOption>
{
    public void Configure(EntityTypeBuilder<NutritionMealOption> builder)
    {
        builder.ToTable("NutritionMealOptions");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Label).HasMaxLength(200).IsRequired();
        builder.Property(o => o.LabelAr).HasMaxLength(200);
        builder.HasOne(o => o.MealBlock).WithMany(b => b.Options).HasForeignKey(o => o.NutritionMealBlockId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(o => new { o.NutritionMealBlockId, o.OrderIndex }).IsUnique();
    }
}

public class NutritionOptionItemMapping : IEntityTypeConfiguration<NutritionOptionItem>
{
    public void Configure(EntityTypeBuilder<NutritionOptionItem> builder)
    {
        builder.ToTable("NutritionOptionItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ItemName).HasMaxLength(300);
        builder.Property(i => i.ItemNameAr).HasMaxLength(300);
        builder.Property(i => i.Quantity).HasPrecision(8, 2);
        builder.Property(i => i.AlternativeGroupKey).HasMaxLength(100);
        builder.HasOne(i => i.Option).WithMany(o => o.Items).HasForeignKey(i => i.NutritionMealOptionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(i => i.Food).WithMany().HasForeignKey(i => i.FoodId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(i => i.Recipe).WithMany().HasForeignKey(i => i.RecipeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(i => new { i.NutritionMealOptionId, i.OrderIndex }).IsUnique();
    }
}

public class NutritionPlanRuleMapping : IEntityTypeConfiguration<NutritionPlanRule>
{
    public void Configure(EntityTypeBuilder<NutritionPlanRule> builder)
    {
        builder.ToTable("NutritionPlanRules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.RuleType).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Text).HasMaxLength(2000);
        builder.Property(r => r.TextAr).HasMaxLength(4000).IsRequired();
        builder.HasOne(r => r.Template).WithMany(t => t.Rules).HasForeignKey(r => r.NutritionPlanTemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(r => new { r.NutritionPlanTemplateId, r.OrderIndex }).IsUnique();
    }
}
