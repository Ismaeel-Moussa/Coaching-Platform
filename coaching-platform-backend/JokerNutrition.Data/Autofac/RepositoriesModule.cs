using Autofac;
using JokerNutrition.Data.Repositories;

namespace JokerNutrition.Data.Autofac;

public class RepositoriesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<AthleteRepository>().As<IAthleteRepository>().InstancePerLifetimeScope();
        builder.RegisterType<CoachRepository>().As<ICoachRepository>().InstancePerLifetimeScope();
        builder.RegisterType<InvitationRepository>().As<IInvitationRepository>().InstancePerLifetimeScope();
        builder.RegisterType<FoodRepository>().As<IFoodRepository>().InstancePerLifetimeScope();
        builder.RegisterType<RecipeRepository>().As<IRecipeRepository>().InstancePerLifetimeScope();
        builder.RegisterType<DailyDiaryRepository>().As<IDailyDiaryRepository>().InstancePerLifetimeScope();
        builder.RegisterType<MealLogRepository>().As<IMealLogRepository>().InstancePerLifetimeScope();
        builder.RegisterType<MacroTargetRepository>().As<IMacroTargetRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ExerciseRepository>().As<IExerciseRepository>().InstancePerLifetimeScope();
        builder.RegisterType<WorkoutTemplateRepository>().As<IWorkoutTemplateRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ClientProgramRepository>().As<IClientProgramRepository>().InstancePerLifetimeScope();
        builder.RegisterType<WorkoutLogRepository>().As<IWorkoutLogRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ExerciseSetLogRepository>().As<IExerciseSetLogRepository>().InstancePerLifetimeScope();
        builder.RegisterType<SupplementScheduleRepository>().As<ISupplementScheduleRepository>().InstancePerLifetimeScope();
        builder.RegisterType<SupplementLogRepository>().As<ISupplementLogRepository>().InstancePerLifetimeScope();
        builder.RegisterType<ClientCheckInRepository>().As<IClientCheckInRepository>().InstancePerLifetimeScope();
        builder.RegisterType<CheckInPhotoRepository>().As<ICheckInPhotoRepository>().InstancePerLifetimeScope();
        builder.RegisterType<CoachFeedbackNoteRepository>().As<ICoachFeedbackNoteRepository>().InstancePerLifetimeScope();
        builder.RegisterType<NotificationRepository>().As<INotificationRepository>().InstancePerLifetimeScope();
        builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
        builder.RegisterType<PasswordResetTokenRepository>().As<IPasswordResetTokenRepository>().InstancePerLifetimeScope();
    }
}
