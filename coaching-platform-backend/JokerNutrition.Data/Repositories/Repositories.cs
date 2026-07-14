using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Entities.Identities;
using Microsoft.Extensions.Logging;

namespace JokerNutrition.Data.Repositories;

// ─── Athlete ───────────────────────────────────────────────────────
public interface IAthleteRepository : _IBaseRepository<Athlete> { }
public class AthleteRepository : _BaseRepository<Athlete>, IAthleteRepository
{
    public AthleteRepository(JokerNutritionContext context, ILogger<AthleteRepository> logger)
        : base(context, logger) { }
}

// ─── Coach ─────────────────────────────────────────────────────────
public interface ICoachRepository : _IBaseRepository<Coach> { }
public class CoachRepository : _BaseRepository<Coach>, ICoachRepository
{
    public CoachRepository(JokerNutritionContext context, ILogger<CoachRepository> logger)
        : base(context, logger) { }
}

// ─── Invitation ────────────────────────────────────────────────────
public interface IInvitationRepository : _IBaseRepository<Invitation> { }
public class InvitationRepository : _BaseRepository<Invitation>, IInvitationRepository
{
    public InvitationRepository(JokerNutritionContext context, ILogger<InvitationRepository> logger)
        : base(context, logger) { }
}

// ─── Food ──────────────────────────────────────────────────────────
public interface IFoodRepository : _IBaseRepository<Food> { }
public class FoodRepository : _BaseRepository<Food>, IFoodRepository
{
    public FoodRepository(JokerNutritionContext context, ILogger<FoodRepository> logger)
        : base(context, logger) { }
}

public interface IFavoriteFoodRepository : _IBaseRepository<FavoriteFood> { }
public class FavoriteFoodRepository : _BaseRepository<FavoriteFood>, IFavoriteFoodRepository
{
    public FavoriteFoodRepository(JokerNutritionContext context, ILogger<FavoriteFoodRepository> logger)
        : base(context, logger) { }
}

// ─── Recipe ────────────────────────────────────────────────────────
public interface IRecipeRepository : _IBaseRepository<Recipe> { }
public class RecipeRepository : _BaseRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(JokerNutritionContext context, ILogger<RecipeRepository> logger)
        : base(context, logger) { }
}

public interface IFavoriteRecipeRepository : _IBaseRepository<FavoriteRecipe> { }
public class FavoriteRecipeRepository : _BaseRepository<FavoriteRecipe>, IFavoriteRecipeRepository
{
    public FavoriteRecipeRepository(JokerNutritionContext context, ILogger<FavoriteRecipeRepository> logger)
        : base(context, logger) { }
}

// ─── DailyDiary ────────────────────────────────────────────────────
public interface IDailyDiaryRepository : _IBaseRepository<DailyDiary> { }
public class DailyDiaryRepository : _BaseRepository<DailyDiary>, IDailyDiaryRepository
{
    public DailyDiaryRepository(JokerNutritionContext context, ILogger<DailyDiaryRepository> logger)
        : base(context, logger) { }
}

// ─── MealLog ───────────────────────────────────────────────────────
public interface IMealLogRepository : _IBaseRepository<MealLog> { }
public class MealLogRepository : _BaseRepository<MealLog>, IMealLogRepository
{
    public MealLogRepository(JokerNutritionContext context, ILogger<MealLogRepository> logger)
        : base(context, logger) { }
}

// ─── MacroTarget ───────────────────────────────────────────────────
public interface IMacroTargetRepository : _IBaseRepository<MacroTarget> { }
public class MacroTargetRepository : _BaseRepository<MacroTarget>, IMacroTargetRepository
{
    public MacroTargetRepository(JokerNutritionContext context, ILogger<MacroTargetRepository> logger)
        : base(context, logger) { }
}

// ─── Exercise ──────────────────────────────────────────────────────
public interface IExerciseRepository : _IBaseRepository<Exercise> { }
public class ExerciseRepository : _BaseRepository<Exercise>, IExerciseRepository
{
    public ExerciseRepository(JokerNutritionContext context, ILogger<ExerciseRepository> logger)
        : base(context, logger) { }
}

// ─── WorkoutTemplate ───────────────────────────────────────────────
public interface IWorkoutTemplateRepository : _IBaseRepository<WorkoutTemplate> { }
public class WorkoutTemplateRepository : _BaseRepository<WorkoutTemplate>, IWorkoutTemplateRepository
{
    public WorkoutTemplateRepository(JokerNutritionContext context, ILogger<WorkoutTemplateRepository> logger)
        : base(context, logger) { }
}

// ─── ClientProgram ─────────────────────────────────────────────────
public interface IClientProgramRepository : _IBaseRepository<ClientProgram> { }
public class ClientProgramRepository : _BaseRepository<ClientProgram>, IClientProgramRepository
{
    public ClientProgramRepository(JokerNutritionContext context, ILogger<ClientProgramRepository> logger)
        : base(context, logger) { }
}

// ─── WorkoutLog ────────────────────────────────────────────────────
public interface IWorkoutLogRepository : _IBaseRepository<WorkoutLog> { }
public class WorkoutLogRepository : _BaseRepository<WorkoutLog>, IWorkoutLogRepository
{
    public WorkoutLogRepository(JokerNutritionContext context, ILogger<WorkoutLogRepository> logger)
        : base(context, logger) { }
}

// ─── ExerciseSetLog ────────────────────────────────────────────────
public interface IExerciseSetLogRepository : _IBaseRepository<ExerciseSetLog> { }
public class ExerciseSetLogRepository : _BaseRepository<ExerciseSetLog>, IExerciseSetLogRepository
{
    public ExerciseSetLogRepository(JokerNutritionContext context, ILogger<ExerciseSetLogRepository> logger)
        : base(context, logger) { }
}

// ─── SupplementSchedule ────────────────────────────────────────────
public interface ISupplementScheduleRepository : _IBaseRepository<SupplementSchedule> { }
public class SupplementScheduleRepository : _BaseRepository<SupplementSchedule>, ISupplementScheduleRepository
{
    public SupplementScheduleRepository(JokerNutritionContext context, ILogger<SupplementScheduleRepository> logger)
        : base(context, logger) { }
}

// ─── SupplementLog ─────────────────────────────────────────────────
public interface ISupplementLogRepository : _IBaseRepository<SupplementLog> { }
public class SupplementLogRepository : _BaseRepository<SupplementLog>, ISupplementLogRepository
{
    public SupplementLogRepository(JokerNutritionContext context, ILogger<SupplementLogRepository> logger)
        : base(context, logger) { }
}

// ─── ClientCheckIn ─────────────────────────────────────────────────
public interface IClientCheckInRepository : _IBaseRepository<ClientCheckIn> { }
public class ClientCheckInRepository : _BaseRepository<ClientCheckIn>, IClientCheckInRepository
{
    public ClientCheckInRepository(JokerNutritionContext context, ILogger<ClientCheckInRepository> logger)
        : base(context, logger) { }
}

// ─── CheckInPhoto ──────────────────────────────────────────────────
public interface ICheckInPhotoRepository : _IBaseRepository<CheckInPhoto> { }
public class CheckInPhotoRepository : _BaseRepository<CheckInPhoto>, ICheckInPhotoRepository
{
    public CheckInPhotoRepository(JokerNutritionContext context, ILogger<CheckInPhotoRepository> logger)
        : base(context, logger) { }
}

// ─── CoachFeedbackNote ─────────────────────────────────────────────
public interface ICoachFeedbackNoteRepository : _IBaseRepository<CoachFeedbackNote> { }
public class CoachFeedbackNoteRepository : _BaseRepository<CoachFeedbackNote>, ICoachFeedbackNoteRepository
{
    public CoachFeedbackNoteRepository(JokerNutritionContext context, ILogger<CoachFeedbackNoteRepository> logger)
        : base(context, logger) { }
}

// ─── Notification ──────────────────────────────────────────────────
public interface INotificationRepository : _IBaseRepository<Notification> { }
public class NotificationRepository : _BaseRepository<Notification>, INotificationRepository
{
    public NotificationRepository(JokerNutritionContext context, ILogger<NotificationRepository> logger)
        : base(context, logger) { }
}

// ─── User ──────────────────────────────────────────────────────────
public interface IUserRepository : _IBaseRepository<User> { }
public class UserRepository : _BaseRepository<User>, IUserRepository
{
    public UserRepository(JokerNutritionContext context, ILogger<UserRepository> logger)
        : base(context, logger) { }
}

// ─── PasswordResetToken ────────────────────────────────────────────
public interface IPasswordResetTokenRepository : _IBaseRepository<PasswordResetToken> { }
public class PasswordResetTokenRepository : _BaseRepository<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(JokerNutritionContext context, ILogger<PasswordResetTokenRepository> logger)
        : base(context, logger) { }
}

// ─── AuditLog ──────────────────────────────────────────────────────
public interface IAuditLogRepository : _IBaseRepository<AuditLog> { }
public class AuditLogRepository : _BaseRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(JokerNutritionContext context, ILogger<AuditLogRepository> logger)
        : base(context, logger) { }
}
