using JokerNutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JokerNutrition.Data.Contexts.ClassMapping;

public class AthleteOnboardingAssessmentMapping : IEntityTypeConfiguration<AthleteOnboardingAssessment>
{
    public void Configure(EntityTypeBuilder<AthleteOnboardingAssessment> builder)
    {
        builder.ToTable("AthleteOnboardingAssessments");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AthleteId).IsUnique();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.PrimaryGoal).HasMaxLength(100);
        builder.Property(x => x.WeightKg).HasPrecision(5, 2);
        builder.Property(x => x.HeightCm).HasPrecision(5, 2);
        builder.Property(x => x.ActivityLevel).HasMaxLength(30);
        builder.Property(x => x.TrainingExperience).HasMaxLength(30);
        builder.Property(x => x.AvailableEquipmentJson).HasColumnType("text").IsRequired();
        builder.Property(x => x.PreferredTrainingDaysJson).HasColumnType("text").IsRequired();
        builder.Property(x => x.InjuriesOrLimitations).HasMaxLength(2000);
        builder.Property(x => x.CurrentPain).HasMaxLength(2000);
        builder.Property(x => x.AverageSleepHours).HasPrecision(3, 1);
        builder.Property(x => x.SleepQuality).HasMaxLength(30);
        builder.Property(x => x.FoodAllergies).HasMaxLength(2000);
        builder.Property(x => x.FoodIntolerances).HasMaxLength(2000);
        builder.Property(x => x.PreferredFoods).HasMaxLength(2000);
        builder.Property(x => x.FoodsToAvoid).HasMaxLength(2000);
        builder.Property(x => x.TypicalMealSchedule).HasMaxLength(1000);
        builder.Property(x => x.CurrentSupplements).HasMaxLength(2000);
        builder.Property(x => x.AdditionalNotes).HasMaxLength(3000);
        builder.Property(x => x.CoachReviewNotes).HasMaxLength(3000);

        builder.HasOne(x => x.Athlete)
            .WithOne(x => x.OnboardingAssessment)
            .HasForeignKey<AthleteOnboardingAssessment>(x => x.AthleteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewedByCoach)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByCoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Photos)
            .WithOne(x => x.OnboardingAssessment)
            .HasForeignKey(x => x.OnboardingAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OnboardingPhotoMapping : IEntityTypeConfiguration<OnboardingPhoto>
{
    public void Configure(EntityTypeBuilder<OnboardingPhoto> builder)
    {
        builder.ToTable("OnboardingPhotos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BlobUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Angle).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.HasOne(x => x.OnboardingAssessment)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.OnboardingAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

