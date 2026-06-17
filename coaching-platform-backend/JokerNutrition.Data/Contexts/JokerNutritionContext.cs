using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JokerNutrition.Data.Contexts;

public class JokerNutritionContext : IdentityDbContext<User, Role, int,
    UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    public JokerNutritionContext(DbContextOptions<JokerNutritionContext> options) : base(options) { }

    public DbSet<Athlete> Athletes => Set<Athlete>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<DailyDiary> DailyDiaries => Set<DailyDiary>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<MacroTarget> MacroTargets => Set<MacroTarget>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<WorkoutTemplateDay> WorkoutTemplateDays => Set<WorkoutTemplateDay>();
    public DbSet<TemplateExercise> TemplateExercises => Set<TemplateExercise>();
    public DbSet<ClientProgram> ClientPrograms => Set<ClientProgram>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<ExerciseSetLog> ExerciseSetLogs => Set<ExerciseSetLog>();
    public DbSet<SupplementSchedule> SupplementSchedules => Set<SupplementSchedule>();
    public DbSet<SupplementLog> SupplementLogs => Set<SupplementLog>();
    public DbSet<ClientCheckIn> ClientCheckIns => Set<ClientCheckIn>();
    public DbSet<CheckInPhoto> CheckInPhotos => Set<CheckInPhoto>();
    public DbSet<CoachFeedbackNote> CoachFeedbackNotes => Set<CoachFeedbackNote>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(JokerNutritionContext).Assembly);
    }
}
