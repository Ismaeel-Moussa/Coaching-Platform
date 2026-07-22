using Autofac;
using JokerNutrition.Business.Helpers;
using JokerNutrition.Business.Reports;
using JokerNutrition.Business.Services;

namespace JokerNutrition.Business.Autofac;

public class ServicesModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<JwtTokenHelper>().As<IJwtTokenHelper>().InstancePerLifetimeScope();
        builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
        builder.RegisterType<AuthService>().As<IAuthService>().InstancePerLifetimeScope();
        builder.RegisterType<InvitationService>().As<IInvitationService>().InstancePerLifetimeScope();
        builder.RegisterType<CacheService>().As<ICacheService>().SingleInstance();

        // ─── Day 2: Nutrition & Diary ────────────────────────────────────
        builder.RegisterType<FoodService>().As<IFoodService>().InstancePerLifetimeScope();
        builder.RegisterType<DiaryService>().As<IDiaryService>().InstancePerLifetimeScope();
        builder.RegisterType<MealLogService>().As<IMealLogService>().InstancePerLifetimeScope();
        builder.RegisterType<RecipeService>().As<IRecipeService>().InstancePerLifetimeScope();
        builder.RegisterType<NutritionPlanService>().As<INutritionPlanService>().InstancePerLifetimeScope();
        builder.RegisterType<AthleteService>().As<IAthleteService>().InstancePerLifetimeScope();

        // ─── Day 3: Workouts & Supplements ───────────────────────────────
        builder.RegisterType<ExerciseService>().As<IExerciseService>().InstancePerLifetimeScope();
        builder.RegisterType<WorkoutLogService>().As<IWorkoutLogService>().InstancePerLifetimeScope();
        builder.RegisterType<SupplementService>().As<ISupplementService>().InstancePerLifetimeScope();

        // ─── Day 4: Coach Operations Hub ─────────────────────────────────
        builder.RegisterType<CoachHubService>().As<ICoachHubService>().InstancePerLifetimeScope();
        builder.RegisterType<AthleteProgressReportService>().As<IAthleteProgressReportService>().InstancePerLifetimeScope();
        builder.RegisterType<ProgressReportPdfGenerator>().As<IProgressReportPdfGenerator>().InstancePerLifetimeScope();
        builder.RegisterType<ProfileService>().As<IProfileService>().InstancePerLifetimeScope();
        builder.RegisterType<BlobStorageService>().As<IBlobStorageService>().InstancePerLifetimeScope();
        builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerLifetimeScope();

        // ─── Day 5: Admin Libraries & Template Builder ────────────────────
        builder.RegisterType<WorkoutTemplateService>().As<IWorkoutTemplateService>().InstancePerLifetimeScope();

        // ─── Day 6: Check-In System & Notifications ───────────────────────
        builder.RegisterType<CheckInService>().As<ICheckInService>().InstancePerLifetimeScope();
        builder.RegisterType<OnboardingAssessmentService>().As<IOnboardingAssessmentService>().InstancePerLifetimeScope();

        // ─── Day 7: Audit Logging & Admin User Management ────────────────
        builder.RegisterType<AuditLogService>().As<IAuditLogService>().InstancePerLifetimeScope();
        builder.RegisterType<AdminUserService>().As<IAdminUserService>().InstancePerLifetimeScope();
    }
}
