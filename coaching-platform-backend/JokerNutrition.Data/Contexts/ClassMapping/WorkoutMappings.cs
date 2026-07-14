using JokerNutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JokerNutrition.Data.Contexts.ClassMapping;

public class ExerciseMapping : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SeedKey).HasMaxLength(200);
        builder.HasIndex(e => e.SeedKey).IsUnique();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.NameAr).HasMaxLength(200);
        builder.Property(e => e.Instructions).HasMaxLength(2000);
        builder.Property(e => e.InstructionsAr).HasMaxLength(4000);
        builder.Property(e => e.EquipmentRequired).HasMaxLength(200);
        builder.Property(e => e.YouTubeVideoId).HasMaxLength(20);
        builder.Property(e => e.VideoUrl).HasMaxLength(1000);
        builder.Property(e => e.SourceDocument).HasMaxLength(300);
        builder.Property(e => e.ContentStatus).HasDefaultValue(JokerNutrition.Data.Enums.ContentStatus.Published);
        builder.Property(e => e.ContentVersion).HasDefaultValue(1);
    }
}

public class WorkoutTemplateMapping : IEntityTypeConfiguration<WorkoutTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplate> builder)
    {
        builder.ToTable("WorkoutTemplates");
        builder.HasKey(wt => wt.Id);
        builder.Property(wt => wt.SeedKey).HasMaxLength(200);
        builder.HasIndex(wt => wt.SeedKey).IsUnique();
        builder.Property(wt => wt.Name).HasMaxLength(200).IsRequired();
        builder.Property(wt => wt.NameAr).HasMaxLength(200);
        builder.Property(wt => wt.Description).HasMaxLength(1000);
        builder.Property(wt => wt.DescriptionAr).HasMaxLength(2000);
        builder.Property(wt => wt.Guidance).HasMaxLength(4000);
        builder.Property(wt => wt.GuidanceAr).HasMaxLength(8000);
        builder.Property(wt => wt.SourceDocument).HasMaxLength(300);
        builder.Property(wt => wt.ContentStatus).HasDefaultValue(JokerNutrition.Data.Enums.ContentStatus.Published);
        builder.Property(wt => wt.ContentVersion).HasDefaultValue(1);
        builder.HasOne(wt => wt.CreatedByCoach).WithMany(c => c.Templates).HasForeignKey(wt => wt.CreatedByCoachId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkoutTemplateDayMapping : IEntityTypeConfiguration<WorkoutTemplateDay>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplateDay> builder)
    {
        builder.ToTable("WorkoutTemplateDays");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.DayLabel).HasMaxLength(100).IsRequired();
        builder.Property(d => d.DayLabelAr).HasMaxLength(100);
        builder.Property(d => d.Instructions).HasMaxLength(2000);
        builder.Property(d => d.InstructionsAr).HasMaxLength(4000);
        builder.Property(d => d.CardioInstructions).HasMaxLength(1000);
        builder.Property(d => d.CardioInstructionsAr).HasMaxLength(2000);
        builder.HasOne(d => d.WorkoutTemplate).WithMany(t => t.Days).HasForeignKey(d => d.WorkoutTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TemplateExerciseMapping : IEntityTypeConfiguration<TemplateExercise>
{
    public void Configure(EntityTypeBuilder<TemplateExercise> builder)
    {
        builder.ToTable("TemplateExercises");
        builder.HasKey(te => te.Id);
        builder.Property(te => te.TargetReps).HasMaxLength(20);
        builder.Property(te => te.ProgressiveOverloadTargetKg).HasPrecision(6, 2);
        builder.Property(te => te.TargetRir).HasPrecision(3, 1);
        builder.Property(te => te.CoachNotes).HasMaxLength(1000);
        builder.Property(te => te.CoachNotesAr).HasMaxLength(2000);
        builder.Property(te => te.AlternativeGroupKey).HasMaxLength(100);
        builder.HasOne(te => te.Day).WithMany(d => d.Exercises).HasForeignKey(te => te.WorkoutTemplateDayId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(te => te.Exercise).WithMany(e => e.TemplateExercises).HasForeignKey(te => te.ExerciseId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ClientProgramMapping : IEntityTypeConfiguration<ClientProgram>
{
    public void Configure(EntityTypeBuilder<ClientProgram> builder)
    {
        builder.ToTable("ClientPrograms");
        builder.HasKey(cp => cp.Id);
        builder.HasOne(cp => cp.Athlete).WithMany(a => a.Programs).HasForeignKey(cp => cp.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(cp => cp.WorkoutTemplate).WithMany(wt => wt.ClientPrograms).HasForeignKey(cp => cp.WorkoutTemplateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(cp => cp.AssignedByCoach).WithMany().HasForeignKey(cp => cp.AssignedByCoachId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class WorkoutLogMapping : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.ToTable("WorkoutLogs");
        builder.HasKey(wl => wl.Id);
        builder.HasOne(wl => wl.Athlete).WithMany().HasForeignKey(wl => wl.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(wl => wl.Day).WithMany().HasForeignKey(wl => wl.WorkoutTemplateDayId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(wl => new { wl.AthleteId, wl.Date });
    }
}

public class ExerciseSetLogMapping : IEntityTypeConfiguration<ExerciseSetLog>
{
    public void Configure(EntityTypeBuilder<ExerciseSetLog> builder)
    {
        builder.ToTable("ExerciseSetLogs");
        builder.HasKey(esl => esl.Id);
        builder.Property(esl => esl.WeightKg).HasPrecision(6, 2);
        builder.HasOne(esl => esl.WorkoutLog).WithMany(wl => wl.Sets).HasForeignKey(esl => esl.WorkoutLogId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(esl => esl.Exercise).WithMany().HasForeignKey(esl => esl.ExerciseId).OnDelete(DeleteBehavior.Restrict);
        // Fast lookup of all sets within a workout session
        builder.HasIndex(esl => esl.WorkoutLogId);
    }
}
