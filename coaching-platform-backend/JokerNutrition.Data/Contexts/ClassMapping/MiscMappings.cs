using JokerNutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JokerNutrition.Data.Contexts.ClassMapping;

public class SupplementScheduleMapping : IEntityTypeConfiguration<SupplementSchedule>
{
    public void Configure(EntityTypeBuilder<SupplementSchedule> builder)
    {
        builder.ToTable("SupplementSchedules");
        builder.HasKey(ss => ss.Id);
        builder.Property(ss => ss.Name).HasMaxLength(200).IsRequired();
        builder.Property(ss => ss.Dosage).HasMaxLength(100);
        builder.Property(ss => ss.Notes).HasMaxLength(500);
        builder.HasOne(ss => ss.Athlete).WithMany(a => a.SupplementSchedules).HasForeignKey(ss => ss.AthleteId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SupplementLogMapping : IEntityTypeConfiguration<SupplementLog>
{
    public void Configure(EntityTypeBuilder<SupplementLog> builder)
    {
        builder.ToTable("SupplementLogs");
        builder.HasKey(sl => sl.Id);
        builder.HasOne(sl => sl.Schedule).WithMany(ss => ss.Logs).HasForeignKey(sl => sl.SupplementScheduleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(sl => new { sl.SupplementScheduleId, sl.Date });
    }
}

public class ClientCheckInMapping : IEntityTypeConfiguration<ClientCheckIn>
{
    public void Configure(EntityTypeBuilder<ClientCheckIn> builder)
    {
        builder.ToTable("ClientCheckIns");
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.WeightKg).HasPrecision(6, 2);
        builder.Property(ci => ci.WaistCm).HasPrecision(6, 2);
        builder.Property(ci => ci.ChestCm).HasPrecision(6, 2);
        builder.Property(ci => ci.ThighCm).HasPrecision(6, 2);
        builder.Property(ci => ci.CoachNotes).HasMaxLength(2000);
        builder.HasOne(ci => ci.Athlete).WithMany(a => a.CheckIns).HasForeignKey(ci => ci.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(ci => new { ci.AthleteId, ci.WeekOf });
    }
}

public class CheckInPhotoMapping : IEntityTypeConfiguration<CheckInPhoto>
{
    public void Configure(EntityTypeBuilder<CheckInPhoto> builder)
    {
        builder.ToTable("CheckInPhotos");
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.BlobUrl).HasMaxLength(500).IsRequired();
        builder.HasOne(cp => cp.CheckIn).WithMany(ci => ci.Photos).HasForeignKey(cp => cp.ClientCheckInId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CoachFeedbackNoteMapping : IEntityTypeConfiguration<CoachFeedbackNote>
{
    public void Configure(EntityTypeBuilder<CoachFeedbackNote> builder)
    {
        builder.ToTable("CoachFeedbackNotes");
        builder.HasKey(cfn => cfn.Id);
        builder.Property(cfn => cfn.NoteText).HasMaxLength(2000).IsRequired();
        builder.HasOne(cfn => cfn.Athlete).WithMany(a => a.FeedbackNotes).HasForeignKey(cfn => cfn.AthleteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(cfn => cfn.Coach).WithMany(c => c.FeedbackNotes).HasForeignKey(cfn => cfn.CoachId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class NotificationMapping : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Message).HasMaxLength(500).IsRequired();
        builder.HasOne(n => n.RecipientUser).WithMany().HasForeignKey(n => n.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
    }
}

public class AuditLogMapping : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(al => al.Id);
        builder.Property(al => al.Action).HasMaxLength(100).IsRequired();
        builder.Property(al => al.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(al => al.EntityId).HasMaxLength(50);
        builder.Property(al => al.Details).HasMaxLength(2000);
    }
}
