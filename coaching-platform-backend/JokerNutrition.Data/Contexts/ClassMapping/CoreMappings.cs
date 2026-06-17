using JokerNutrition.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JokerNutrition.Data.Contexts.ClassMapping;

public class AthleteMapping : IEntityTypeConfiguration<Athlete>
{
    public void Configure(EntityTypeBuilder<Athlete> builder)
    {
        builder.ToTable("Athletes");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.TargetGoal).HasMaxLength(100);
        builder.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.AssignedCoach).WithMany(c => c.Athletes).HasForeignKey(a => a.AssignedCoachId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CoachMapping : IEntityTypeConfiguration<Coach>
{
    public void Configure(EntityTypeBuilder<Coach> builder)
    {
        builder.ToTable("Coaches");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Bio).HasMaxLength(1000);
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class InvitationMapping : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Email).HasMaxLength(256).IsRequired();
        builder.Property(i => i.Token).HasMaxLength(500).IsRequired();
        builder.Property(i => i.Role).HasMaxLength(50).IsRequired();
        builder.HasOne(i => i.IssuedBy).WithMany().HasForeignKey(i => i.IssuedByCoachId).OnDelete(DeleteBehavior.Restrict);
    }
}
