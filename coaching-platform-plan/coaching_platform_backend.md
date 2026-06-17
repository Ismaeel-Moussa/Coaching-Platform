# Joker Nutrition Platform – Backend Build Prompt & Architecture Plan

> **Goal**: Build the **Joker Nutrition** ASP.NET Core Web API using the **exact same 3-layer architecture, design patterns, and NuGet packages** as the 88ninety Academy project. Every convention – folder layout, base classes, Autofac modules, JWT auth, Serilog, EF Core fluent mapping, custom exception filter, pagination, etc. – must be reproduced faithfully. The domain, however, is a **high-performance nutrition & fitness coaching platform** with two user hubs: an **Athlete (Client) Hub** and a **Coach/Admin Hub**.

---

## 1. Solution Structure

Create a Visual Studio solution named **`JokerNutrition.sln`** with **three class-library / web projects**:

```
JokerNutrition/
├── JokerNutrition.sln
├── JokerNutrition.Api/          ← ASP.NET Core Web API (SDK: Microsoft.NET.Sdk.Web)
├── JokerNutrition.Business/     ← Class Library (SDK: Microsoft.NET.Sdk)
└── JokerNutrition.Data/         ← Class Library (SDK: Microsoft.NET.Sdk)
```

**Project references (same direction as Academy):**
```
Api → Business → Data
```

---

## 2. Target Framework & Root Namespaces

| Project | TargetFramework | RootNamespace |
|---|---|---|
| JokerNutrition.Api | net8.0 | JokerNutrition.Api |
| JokerNutrition.Business | net8.0 | JokerNutrition.Business |
| JokerNutrition.Data | net8.0 | JokerNutrition.Data |

All three: `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`

---

## 3. NuGet Packages

### 3.1 JokerNutrition.Api.csproj
```xml
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
<PackageReference Include="Autofac" Version="8.4.0" />
<PackageReference Include="Autofac.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="AutoWrapper.Core" Version="4.5.1" />
<PackageReference Include="MailKit" Version="4.16.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.7" />
<PackageReference Include="Microsoft.AspNetCore.Authorization" Version="8.0.7" />
<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.7" />
<PackageReference Include="Microsoft.Extensions.Identity.Core" Version="8.0.7" />
<PackageReference Include="Serilog" Version="4.3.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="8.1.4" />
<PackageReference Include="Swashbuckle.AspNetCore.Swagger" Version="8.1.4" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" Version="8.1.4" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="8.1.4" />
```

### 3.2 JokerNutrition.Business.csproj
```xml
<FrameworkReference Include="Microsoft.AspNetCore.App" />
<PackageReference Include="Autofac" Version="8.4.0" />
<PackageReference Include="Autofac.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="Azure.Storage.Blobs" Version="12.20.0" />
<PackageReference Include="MailKit" Version="4.16.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity" Version="2.3.1" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="PuppeteerSharp" Version="24.40.0" />
```

### 3.3 JokerNutrition.Data.csproj
```xml
<PackageReference Include="Autofac" Version="8.4.0" />
<PackageReference Include="Autofac.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.7" />
```

---

## 4. Data Layer – `JokerNutrition.Data`

### 4.1 Folder Structure
```
JokerNutrition.Data/
├── Autofac/
│   └── RepositoriesModule.cs          ← Autofac module: registers all repositories
├── Contexts/
│   ├── JokerNutritionContext.cs       ← DbContext (inherits IdentityDbContext<User, Role, int>)
│   └── ClassMapping/                  ← Fluent API entity configurations (IEntityTypeConfiguration<T>)
│       ├── UserMapping.cs
│       ├── AthleteMapping.cs
│       ├── CoachMapping.cs
│       ├── InvitationMapping.cs
│       ├── FoodMapping.cs
│       ├── RecipeMapping.cs
│       ├── RecipeIngredientMapping.cs
│       ├── DailyDiaryMapping.cs
│       ├── MealLogMapping.cs
│       ├── MacroTargetMapping.cs
│       ├── ExerciseMapping.cs
│       ├── WorkoutTemplateMapping.cs
│       ├── WorkoutTemplateDayMapping.cs
│       ├── TemplateExerciseMapping.cs
│       ├── ClientProgramMapping.cs
│       ├── WorkoutLogMapping.cs
│       ├── ExerciseSetLogMapping.cs
│       ├── SupplementScheduleMapping.cs
│       ├── SupplementLogMapping.cs
│       ├── ClientCheckInMapping.cs
│       ├── CheckInPhotoMapping.cs
│       ├── CoachFeedbackNoteMapping.cs
│       ├── NotificationMapping.cs
│       └── AuditLogMapping.cs
├── Entities/
│   ├── Identities/
│   │   ├── User.cs                    ← extends IdentityUser<int>
│   │   ├── Role.cs                    ← extends IdentityRole<int>
│   │   ├── UserRole.cs
│   │   ├── UserClaim.cs
│   │   ├── UserLogin.cs
│   │   ├── UserToken.cs
│   │   ├── RoleClaim.cs
│   │   └── PasswordResetToken.cs
│   ├── Athlete.cs                     ← Athlete profile linked to User
│   ├── Coach.cs                       ← Coach profile linked to User
│   ├── Invitation.cs                  ← Onboarding invitation links
│   ├── Food.cs                        ← Global food/ingredient database
│   ├── Recipe.cs                      ← Custom & Joker-curated recipes
│   ├── RecipeIngredient.cs            ← Ingredients within a recipe
│   ├── DailyDiary.cs                  ← Athlete's daily tracking record
│   ├── MealLog.cs                     ← Individual food entry in a diary
│   ├── MacroTarget.cs                 ← Coach-assigned daily macro/calorie targets
│   ├── Exercise.cs                    ← Global exercise library
│   ├── WorkoutTemplate.cs             ← 6-day PPL workout template (coach-built)
│   ├── WorkoutTemplateDay.cs          ← One day slot within a template
│   ├── TemplateExercise.cs            ← Exercise assigned to a template day
│   ├── ClientProgram.cs               ← Assigned template instance for an athlete
│   ├── WorkoutLog.cs                  ← Athlete's daily workout session log
│   ├── ExerciseSetLog.cs              ← Per-set logging (weight, reps)
│   ├── SupplementSchedule.cs          ← Supplement plan assigned to athlete
│   ├── SupplementLog.cs               ← Daily supplement check-off record
│   ├── ClientCheckIn.cs               ← Weekly biometric check-in submission
│   ├── CheckInPhoto.cs                ← Progress photo (Front/Side/Back) per check-in
│   ├── CoachFeedbackNote.cs           ← Coach's written feedback on an athlete
│   ├── Notification.cs                ← In-app notification events
│   └── AuditLog.cs                    ← Immutable audit trail for sensitive changes
├── Extensions/
│   └── SeedExtensions.cs              ← Mock data seeding (called in dev mode)
├── Migrations/                        ← EF Core migrations (auto-generated)
└── Repositories/
    ├── _BaseRepository.cs
    ├── AthleteRepository.cs
    ├── CoachRepository.cs
    ├── InvitationRepository.cs
    ├── FoodRepository.cs
    ├── RecipeRepository.cs
    ├── DailyDiaryRepository.cs
    ├── MealLogRepository.cs
    ├── MacroTargetRepository.cs
    ├── ExerciseRepository.cs
    ├── WorkoutTemplateRepository.cs
    ├── ClientProgramRepository.cs
    ├── WorkoutLogRepository.cs
    ├── ExerciseSetLogRepository.cs
    ├── SupplementScheduleRepository.cs
    ├── SupplementLogRepository.cs
    ├── ClientCheckInRepository.cs
    ├── CheckInPhotoRepository.cs
    ├── CoachFeedbackNoteRepository.cs
    ├── NotificationRepository.cs
    ├── UserRepository.cs
    └── PasswordResetTokenRepository.cs
```

### 4.2 Domain Entities (Joker Nutrition)

#### User (Identity)
```csharp
// Entities/Identities/User.cs
public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
```

#### Athlete
```csharp
public class Athlete
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? AssignedCoachId { get; set; }
    public Coach? AssignedCoach { get; set; }

    // Baseline measurements set during onboarding (Join the Team)
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public string? TargetGoal { get; set; }          // e.g. "Muscle Gain", "Fat Loss", "Recomposition"
    public bool RamadanModeEnabled { get; set; } = false;

    // Streak tracking
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateTime? LastWorkoutDate { get; set; }

    public ICollection<DailyDiary> Diaries { get; set; } = new List<DailyDiary>();
    public ICollection<ClientProgram> Programs { get; set; } = new List<ClientProgram>();
    public ICollection<ClientCheckIn> CheckIns { get; set; } = new List<ClientCheckIn>();
    public ICollection<MacroTarget> MacroTargets { get; set; } = new List<MacroTarget>();
    public ICollection<SupplementSchedule> SupplementSchedules { get; set; } = new List<SupplementSchedule>();
    public ICollection<CoachFeedbackNote> FeedbackNotes { get; set; } = new List<CoachFeedbackNote>();
}
```

#### Coach
```csharp
public class Coach
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Athlete> Athletes { get; set; } = new List<Athlete>();
    public ICollection<WorkoutTemplate> Templates { get; set; } = new List<WorkoutTemplate>();
    public ICollection<CoachFeedbackNote> FeedbackNotes { get; set; } = new List<CoachFeedbackNote>();
}
```

#### Invitation
```csharp
public class Invitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;         // Unique GUID link token
    public string Role { get; set; } = "Athlete";              // "Athlete" | "Coach" | "Admin"
    public InvitationStatus Status { get; set; }               // Pending, Accepted, Expired, Revoked
    public int IssuedByCoachId { get; set; }
    public Coach IssuedBy { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### Food
```csharp
public class Food
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }                      // e.g. "Protein", "Carb", "Fat", "Dairy"
    public bool IsCustom { get; set; } = false;                // true = user-created
    // Macros per 100g
    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public decimal FatPer100g { get; set; }
    public decimal FiberPer100g { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
```

#### Recipe & RecipeIngredient
```csharp
public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RecipeCategory Category { get; set; }               // MuscleBuilding, FatLoss, Custom
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public bool IsJokerRecipe { get; set; } = false;           // true = admin-curated Joker recipe
    public int? CreatedByAthleteId { get; set; }
    public Athlete? CreatedByAthlete { get; set; }
    public DateTime CreatedAt { get; set; }

    // Calculated totals (stored for fast reads)
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFat { get; set; }

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
}

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int FoodId { get; set; }
    public Food Food { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
    public FoodState State { get; set; }                       // Raw, Cooked, Dry
}
```

#### DailyDiary & MealLog
```csharp
public class DailyDiary
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public DateOnly Date { get; set; }

    // Daily targets snapshot (copied from MacroTarget at diary creation)
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }

    // Hydration & steps tracking
    public decimal WaterLitersConsumed { get; set; } = 0;
    public decimal WaterLitersTarget { get; set; } = 4.0m;
    public int StepsWalked { get; set; } = 0;
    public int StepsTarget { get; set; } = 7000;

    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
}

public class MealLog
{
    public int Id { get; set; }
    public int DailyDiaryId { get; set; }
    public DailyDiary DailyDiary { get; set; } = null!;
    public int? FoodId { get; set; }
    public Food? Food { get; set; }
    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public MealType MealType { get; set; }                     // Breakfast, Lunch, Dinner, Snack (or Ramadan variants)
    public decimal QuantityGrams { get; set; }
    public FoodState State { get; set; }                       // Raw, Cooked, Dry

    // Calculated at log time
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fat { get; set; }
    public DateTime LoggedAt { get; set; }
}
```

#### MacroTarget
```csharp
public class MacroTarget
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int SetByCoachId { get; set; }
    public Coach SetByCoach { get; set; } = null!;
    public decimal TargetCalories { get; set; }
    public decimal TargetProtein { get; set; }
    public decimal TargetCarbs { get; set; }
    public decimal TargetFat { get; set; }
    public decimal WaterLitersTarget { get; set; } = 4.0m;
    public int StepsTarget { get; set; } = 7000;
    public bool IsActive { get; set; } = true;
    public DateTime SetAt { get; set; }
}
```

#### Exercise
```csharp
public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public MuscleGroup PrimaryMuscle { get; set; }             // Chest, Back, Shoulders, Arms, Legs, Cardio, Core
    public string? EquipmentRequired { get; set; }
    public string? YouTubeVideoId { get; set; }                // YouTube video hash for demo overlay
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<TemplateExercise> TemplateExercises { get; set; } = new List<TemplateExercise>();
}
```

#### WorkoutTemplate, WorkoutTemplateDay & TemplateExercise
```csharp
public class WorkoutTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreatedByCoachId { get; set; }
    public Coach CreatedByCoach { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<WorkoutTemplateDay> Days { get; set; } = new List<WorkoutTemplateDay>();
    public ICollection<ClientProgram> ClientPrograms { get; set; } = new List<ClientProgram>();
}

public class WorkoutTemplateDay
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int DayNumber { get; set; }                         // 1-6 (1=Push Day 1, 2=Pull Day 1, etc.)
    public string DayLabel { get; set; } = string.Empty;       // "Push Day 1", "Pull Day 1", "Rest", etc.
    public bool IsRestDay { get; set; } = false;

    public ICollection<TemplateExercise> Exercises { get; set; } = new List<TemplateExercise>();
}

public class TemplateExercise
{
    public int Id { get; set; }
    public int WorkoutTemplateDayId { get; set; }
    public WorkoutTemplateDay Day { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public ExerciseSection Section { get; set; }               // WarmUp, Main, CoolDown
    public int OrderIndex { get; set; }
    public int TargetSets { get; set; }
    public string TargetReps { get; set; } = string.Empty;     // e.g. "8-12" or "15"
    public int? RestSeconds { get; set; }
    public bool IsSupersetWith { get; set; } = false;
    public int? SupersetGroupId { get; set; }
    public decimal? ProgressiveOverloadTargetKg { get; set; }
}
```

#### ClientProgram & WorkoutLog
```csharp
public class ClientProgram
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int AssignedByCoachId { get; set; }
    public Coach AssignedByCoach { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class WorkoutLog
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int WorkoutTemplateDayId { get; set; }
    public WorkoutTemplateDay Day { get; set; } = null!;
    public DateOnly Date { get; set; }
    public WorkoutStatus Status { get; set; }                  // InProgress, Completed, Missed
    public DateTime? CompletedAt { get; set; }

    public ICollection<ExerciseSetLog> Sets { get; set; } = new List<ExerciseSetLog>();
}

public class ExerciseSetLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public decimal WeightKg { get; set; }
    public int Reps { get; set; }
    public bool IsCompleted { get; set; } = false;
}
```

#### Supplements
```csharp
public class SupplementSchedule
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public string Name { get; set; } = string.Empty;           // e.g. "Creatine", "Omega-3"
    public SupplementType Type { get; set; }                   // Essential, Optional
    public string? Dosage { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SupplementLog> Logs { get; set; } = new List<SupplementLog>();
}

public class SupplementLog
{
    public int Id { get; set; }
    public int SupplementScheduleId { get; set; }
    public SupplementSchedule Schedule { get; set; } = null!;
    public DateOnly Date { get; set; }
    public bool IsTaken { get; set; } = false;
    public DateTime? TakenAt { get; set; }
}
```

#### ClientCheckIn & CheckInPhoto
```csharp
public class ClientCheckIn
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public DateOnly WeekOf { get; set; }                       // Start of the check-in week
    public DateTime SubmittedAt { get; set; }

    // Biometrics
    public decimal WeightKg { get; set; }
    public decimal? WaistCm { get; set; }
    public decimal? ChestCm { get; set; }
    public decimal? ThighCm { get; set; }

    // Subjective 1-10 slider scores
    public int SleepQuality { get; set; }
    public int EnergyLevel { get; set; }
    public int GutHealth { get; set; }
    public int TrainingStress { get; set; }

    // Coach response
    public string? CoachNotes { get; set; }
    public DateTime? CoachReviewedAt { get; set; }

    public ICollection<CheckInPhoto> Photos { get; set; } = new List<CheckInPhoto>();
}

public class CheckInPhoto
{
    public int Id { get; set; }
    public int ClientCheckInId { get; set; }
    public ClientCheckIn CheckIn { get; set; } = null!;
    public PhotoAngle Angle { get; set; }                      // Front, Side, Back
    public string BlobUrl { get; set; } = string.Empty;        // Azure Blob Storage URL
    public DateTime UploadedAt { get; set; }
}
```

#### CoachFeedbackNote & Notification
```csharp
public class CoachFeedbackNote
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public string NoteText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class Notification
{
    public int Id { get; set; }
    public int RecipientUserId { get; set; }
    public User RecipientUser { get; set; } = null!;
    public NotificationType Type { get; set; }                 // CheckInSubmitted, WorkoutCompleted, CoachNote, etc.
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
```

### 4.3 Base Repository (copy pattern exactly)
```csharp
// Repositories/_BaseRepository.cs
public abstract class _BaseRepository<TEntity> : _IBaseRepository<TEntity>
    where TEntity : class
{
    protected readonly JokerNutritionContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger _logger;

    public _BaseRepository(JokerNutritionContext context, ILogger logger) { ... }

    // Implement: Get, Query, QueryAll, GetById, GetByIdAsync,
    //            Create, CreateRangeAsync, Delete, Update,
    //            SaveChanges, SaveChangesAsync
}

public interface _IBaseRepository<TEntity> where TEntity : class { ... }
```

### 4.4 Autofac RepositoriesModule
```csharp
// Autofac/RepositoriesModule.cs
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
```

---

## 5. Business Layer – `JokerNutrition.Business`

### 5.1 Folder Structure
```
JokerNutrition.Business/
├── Autofac/
│   └── ServicesModule.cs           ← registers all services
├── Common/
│   ├── BasePaginationForm.cs       ← { int Page, int PageSize }
│   └── PagedResult.cs              ← { IEnumerable<T> Items, int TotalCount, int Page, int PageSize }
├── Configurations/
│   ├── JwtSettings.cs
│   ├── SmtpSettings.cs
│   ├── BlobStorageSettings.cs
│   └── AppSettings.cs
├── DTOs/
│   ├── Auth/
│   ├── Athletes/
│   ├── Coaches/
│   ├── Invitations/
│   ├── Foods/
│   ├── Recipes/
│   ├── Diary/
│   ├── Workouts/
│   ├── Supplements/
│   ├── CheckIns/
│   ├── Coach/                      ← Coach hub aggregate DTOs (dashboard, roster, compliance)
│   └── Users/
├── Enums/
│   ├── MealType.cs                 ← Breakfast, Lunch, Dinner, Snack, Suhoor, Iftar, PreWorkout, PostWorkout
│   ├── FoodState.cs                ← Raw, Cooked, Dry
│   ├── RecipeCategory.cs           ← MuscleBuilding, FatLoss, Custom
│   ├── MuscleGroup.cs              ← Chest, Back, Shoulders, Arms, Legs, Cardio, Core
│   ├── ExerciseSection.cs          ← WarmUp, Main, CoolDown
│   ├── WorkoutStatus.cs            ← InProgress, Completed, Missed
│   ├── SupplementType.cs           ← Essential, Optional
│   ├── PhotoAngle.cs               ← Front, Side, Back
│   ├── InvitationStatus.cs         ← Pending, Accepted, Expired, Revoked
│   └── NotificationType.cs         ← CheckInSubmitted, WorkoutCompleted, CoachNote, MacroAlert
├── Exceptions/
│   └── (domain-specific exceptions)
├── Forms/
│   ├── Auth/
│   ├── Athletes/
│   ├── Invitations/
│   ├── Foods/
│   ├── Recipes/
│   ├── Diary/
│   ├── Workouts/
│   ├── Supplements/
│   └── CheckIns/
├── Helpers/
│   ├── JwtTokenHelper.cs
│   └── MacroCalculatorHelper.cs    ← Calculates macros from food weight & state (Raw/Cooked/Dry)
├── Mappers/
│   ├── AthleteMapper.cs
│   ├── CoachMapper.cs
│   ├── InvitationMapper.cs
│   ├── FoodMapper.cs
│   ├── RecipeMapper.cs
│   ├── DiaryMapper.cs
│   ├── MealLogMapper.cs
│   ├── ExerciseMapper.cs
│   ├── WorkoutTemplateMapper.cs
│   ├── WorkoutLogMapper.cs
│   ├── SupplementMapper.cs
│   ├── CheckInMapper.cs
│   ├── AuthMapper.cs
│   ├── UserMapper.cs
│   └── PaginationMapper.cs
├── Security/
│   ├── AppPrincipal.cs
│   └── AppClaimsTransformation.cs
├── Services/
│   ├── _BaseService.cs             ← abstract base (IPrincipal + ILogger)
│   ├── AuthService.cs
│   ├── AccountService.cs
│   ├── ProfileService.cs
│   ├── UserService.cs
│   ├── AthleteService.cs           ← Athlete profile, baseline, streak management
│   ├── CoachService.cs             ← Coach profile, roster management
│   ├── InvitationService.cs        ← Create/resend/revoke invitation links
│   ├── FoodService.cs              ← Food search, bulk import
│   ├── RecipeService.cs            ← Custom & Joker recipes, quick-add to diary
│   ├── DiaryService.cs             ← Daily diary CRUD, macro summaries
│   ├── MealLogService.cs           ← Add/remove individual food/recipe log entries
│   ├── MacroTargetService.cs       ← Coach sets macro/calorie targets for athlete
│   ├── ExerciseService.cs          ← Exercise library CRUD
│   ├── WorkoutTemplateService.cs   ← Template builder: create, assign, manage
│   ├── WorkoutLogService.cs        ← Log sets, complete workout, update streak
│   ├── SupplementService.cs        ← Schedule & daily check-off
│   ├── CheckInService.cs           ← Submit check-in, upload photos, coach feedback
│   ├── CoachHubService.cs          ← Dashboard stats, live feed, compliance roster
│   ├── NotificationService.cs      ← In-app alerts
│   ├── EmailService.cs
│   ├── BlobStorageService.cs       ← Azure Blob: photo upload pre-signed URLs, profile pics
│   └── PasswordResetService.cs
└── Validations/
    ├── FormValidator.cs
    └── FormValidatorResults.cs
```

### 5.2 Base Service (copy pattern exactly)
```csharp
// Services/_BaseService.cs
public abstract class _BaseService
{
    protected readonly IPrincipal _principal;
    protected readonly ILogger _logger;

    public _BaseService(IPrincipal principal, ILogger logger)
    {
        _logger = logger;
        _principal = principal;
    }

    private AppPrincipal _user;

    protected AppPrincipal LoggedInUser
    {
        get
        {
            if (_user != null) return _user;
            var principal = _principal as ClaimsPrincipal
                ?? throw new UnauthorizedAccessException("Unable to find logged in person information");
            _user = AppClaimsTransformation.Transform(principal);
            return _user;
        }
    }
}
```

### 5.3 Key Service Contracts

| Service | Interface | Key Methods |
|---|---|---|
| `AthleteService` | `IAthleteService` | GetAthleteProfile, UpdateBaseline, GetStreak, GetDashboardSummary |
| `CoachService` | `ICoachService` | GetRoster (paged), GetAthleteDeepProfile, GetCoachStats |
| `InvitationService` | `IInvitationService` | CreateInvitation, ListInvitations, ResendInvitation, RevokeInvitation, ValidateToken |
| `FoodService` | `IFoodService` | SearchFoods, GetFoodById, CreateFood, BulkImportFoods |
| `RecipeService` | `IRecipeService` | GetRecipes (tabbed), CreateRecipe, GetById, QuickAddToDiary |
| `DiaryService` | `IDiaryService` | GetDiaryForDate, GetMacroSummary, UpdateWater, UpdateSteps |
| `MealLogService` | `IMealLogService` | LogFood, LogRecipe, RemoveLogEntry |
| `MacroTargetService` | `IMacroTargetService` | SetTargets, GetActiveTargets |
| `ExerciseService` | `IExerciseService` | GetExercises (filtered), CreateExercise, UpdateExercise, DeleteExercise |
| `WorkoutTemplateService` | `IWorkoutTemplateService` | GetTemplates, CreateTemplate, UpdateTemplate, AssignToAthletes |
| `WorkoutLogService` | `IWorkoutLogService` | GetTodaysWorkout, LogSet, CompleteWorkout, GetWorkoutHistory |
| `SupplementService` | `ISupplementService` | GetSchedule, ToggleTaken, GetTodayStatus |
| `CheckInService` | `ICheckInService` | SubmitCheckIn, GetUploadUrl, GetCheckInHistory, AddCoachNotes, GetPendingCheckIns |
| `CoachHubService` | `ICoachHubService` | GetDashboardStats, GetLiveFeed, GetComplianceRoster, GetAthleteWeightHistory |
| `BlobStorageService` | `IBlobStorageService` | GeneratePresignedUploadUrl, GeneratePresignedDownloadUrl |

---

## 6. API Layer – `JokerNutrition.Api`

### 6.1 Folder Structure
```
JokerNutrition.Api/
├── Autofac/
│   └── ControllersModule.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── AccountController.cs
│   ├── ProfileController.cs
│   ├── UsersController.cs
│   ├── InvitationsController.cs          ← Invitation lifecycle (issue, resend, revoke)
│   ├── AthletesController.cs             ← Athlete profile & dashboard
│   ├── CoachesController.cs              ← Coach profile management
│   ├── FoodsController.cs                ← Food search & admin CRUD
│   ├── RecipesController.cs              ← Recipe library
│   ├── DiaryController.cs                ← Daily diary & meal logging
│   ├── MacroTargetsController.cs         ← Coach sets athlete macro targets
│   ├── ExercisesController.cs            ← Exercise library admin
│   ├── WorkoutTemplatesController.cs     ← Template builder & assignment
│   ├── WorkoutLogsController.cs          ← Set logging & workout completion
│   ├── SupplementsController.cs          ← Supplement schedule & daily check-off
│   ├── CheckInsController.cs             ← Weekly check-ins & photo upload URLs
│   ├── CoachHubController.cs             ← Coach dashboard, live feed, compliance, roster
│   ├── NotificationsController.cs        ← In-app alert management
│   └── FilesController.cs                ← General file/blob operations
├── Extensions/
│   └── SeedExtensionMethods.cs           ← app.SeedMockDataAsync()
├── Filters/
│   └── ApiExceptionFilter.cs
├── Logs/
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

### 6.2 Controller Pattern

All controllers follow the same pattern as the Academy project:

```csharp
[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(ApiExceptionFilter))]
public class DiaryController : ControllerBase
{
    private readonly IDiaryService _diaryService;
    private readonly IMealLogService _mealLogService;

    public DiaryController(IDiaryService diaryService, IMealLogService mealLogService)
    {
        _diaryService = diaryService;
        _mealLogService = mealLogService;
    }

    [HttpGet("{date}")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetDiary(DateOnly date) { ... }

    [HttpPost("log")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> LogFood([FromBody] LogFoodForm form) { ... }

    [HttpDelete("log/{id}")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> RemoveEntry(int id) { ... }

    [HttpGet("summary/{date}")]
    [Authorize(Roles = "Athlete")]
    public async Task<IActionResult> GetMacroSummary(DateOnly date) { ... }
}
```

### 6.3 API Endpoint Reference

#### Auth & Invitations
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | Public | Register via invitation token |
| POST | `/api/auth/login` | Public | Login, receive JWT + refresh token |
| POST | `/api/auth/refresh` | Public | Rotate refresh token |
| POST | `/api/auth/forgot-password` | Public | Request password reset email |
| POST | `/api/auth/reset-password` | Public | Reset password with token |
| GET | `/api/invitations` | Coach/Admin | List all invitations |
| POST | `/api/invitations` | Coach/Admin | Create & send invitation |
| POST | `/api/invitations/resend/{id}` | Coach/Admin | Resend invitation email |
| DELETE | `/api/invitations/{id}` | Coach/Admin | Revoke invitation |
| GET | `/api/invitations/validate/{token}` | Public | Validate invite token for registration |

#### Athlete Hub
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/athletes/me/dashboard` | Athlete | Dashboard summary: macros, today's session, streak |
| GET | `/api/athletes/me/targets` | Athlete | Active macro & daily targets |
| GET | `/api/athletes/{id}/targets` | Coach/Admin | Get targets for a specific athlete |
| POST | `/api/athletes/{id}/targets` | Coach/Admin | Set macro targets for athlete |
| GET | `/api/diary/{date}` | Athlete | Full diary for date (all meal types + logs) |
| POST | `/api/diary/log` | Athlete | Log a food or recipe entry |
| DELETE | `/api/diary/log/{id}` | Athlete | Remove a log entry |
| GET | `/api/diary/summary/{date}` | Athlete | Macro totals vs. target for the day |
| PATCH | `/api/diary/{date}/water` | Athlete | Update water consumed |
| PATCH | `/api/diary/{date}/steps` | Athlete | Update steps walked |

#### Food & Recipes
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/foods` | Athlete/Coach | Search foods by name or category |
| GET | `/api/foods/{id}` | Athlete/Coach | Single food details |
| POST | `/api/foods` | Admin | Create new food entry |
| PUT | `/api/foods/{id}` | Admin | Update food |
| DELETE | `/api/foods/{id}` | Admin | Soft-delete food |
| POST | `/api/foods/bulk-import` | Admin | CSV batch import |
| GET | `/api/recipes` | Athlete/Coach | List recipes (tabbed: MuscleBuilding, FatLoss, Custom) |
| GET | `/api/recipes/{id}` | Athlete/Coach | Single recipe with ingredients |
| POST | `/api/recipes` | Athlete/Admin | Create recipe |
| POST | `/api/recipes/{id}/add-to-diary` | Athlete | Quick-add full recipe to today's diary |
| DELETE | `/api/recipes/{id}` | Admin | Delete Joker recipe |

#### Workout System
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/workout-templates` | Coach/Admin | List all workout templates |
| POST | `/api/workout-templates` | Coach/Admin | Create new 6-day workout template |
| PUT | `/api/workout-templates/{id}` | Coach/Admin | Update template |
| POST | `/api/workout-templates/{id}/assign` | Coach/Admin | Assign template to one or more athletes |
| GET | `/api/workouts/today` | Athlete | Today's scheduled workout with exercises |
| GET | `/api/workouts/program` | Athlete | Full 6-day split for current athlete |
| POST | `/api/workouts/log-set` | Athlete | Log a completed set (exerciseId, weight, reps) |
| POST | `/api/workouts/complete` | Athlete | Mark today's workout as completed, update streak |
| GET | `/api/workouts/history` | Athlete | Exercise history for progressive overload |

#### Exercises
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/exercises` | All | List exercises with category filters + pagination |
| GET | `/api/exercises/{id}` | All | Single exercise details |
| POST | `/api/exercises` | Coach/Admin | Create exercise |
| PUT | `/api/exercises/{id}` | Coach/Admin | Update exercise |
| DELETE | `/api/exercises/{id}` | Coach/Admin | Soft-delete exercise |

#### Supplements
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/supplements` | Athlete | Supplement schedule with today's status |
| POST | `/api/supplements/log` | Athlete | Toggle supplement as taken/untaken |
| POST | `/api/supplements/schedule` | Coach/Admin | Assign supplement plan to athlete |

#### Check-Ins
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/checkins` | Athlete | Submit weekly biometric check-in |
| POST | `/api/checkins/{id}/photos/upload-url` | Athlete | Get pre-signed URL for photo upload (Front/Side/Back) |
| GET | `/api/checkins/history` | Athlete | Full check-in history |
| GET | `/api/checkins/pending` | Coach/Admin | Athletes with no check-in this week |
| PUT | `/api/checkins/{id}/coach-notes` | Coach/Admin | Add coach feedback to a check-in |
| GET | `/api/checkins/{id}/photos` | Coach/Admin | Signed download URLs for check-in photos |

#### Coach Hub
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/coach-hub/dashboard` | Coach/Admin | KPIs: active athletes, avg workout %, pending check-ins |
| GET | `/api/coach-hub/live-feed` | Coach/Admin | Paginated real-time workout events across all roster athletes |
| GET | `/api/coach-hub/compliance` | Coach/Admin | Per-athlete calorie & macro compliance for today |
| GET | `/api/coach-hub/roster` | Coach/Admin | Full roster with last check-in, compliance %, program |
| GET | `/api/coach-hub/athletes/{id}` | Coach/Admin | Deep athlete profile: biometrics history, macro logs, workout history |
| POST | `/api/coach-hub/athletes/{id}/notes` | Coach/Admin | Save coach feedback note |
| GET | `/api/coach-hub/athletes/{id}/weight-history` | Coach/Admin | Weight measurement series for chart |

### 6.4 Program.cs – Full Bootstrap Checklist

Replicate **exactly** the same middleware pipeline and registration order:

```
1. AddEndpointsApiExplorer + AddSwaggerGen (with Bearer JWT definition)
2. AddCors (AllowSpecificOrigin → http://localhost:5173)
3. Serilog (Log.Logger → File sink, Warning level, rolling daily)
4. AddIdentity<User, Role> (password policy)
5. AddDbContext<JokerNutritionContext> (SQL Server)
6. AddMemoryCache
7. UseServiceProviderFactory(AutofacServiceProviderFactory)
8. ConfigureContainer<ContainerBuilder>
   - RegisterType<JokerNutritionContext>
   - RegisterType<JwtTokenHelper>.As<IJwtTokenHelper>
   - Register<IPrincipal> via IHttpContextAccessor
   - RegisterModule<RepositoriesModule>
   - RegisterModule<ServicesModule>
   - RegisterModule<ControllersModule>
9. UseSerilog
10. Configure<JwtSettings>, Configure<SmtpSettings>, Configure<BlobStorageSettings>, Configure<AppSettings>
11. AddAuthentication(JwtBearer) + AddJwtBearer (validate issuer, audience, lifetime, signing key)
12. AddControllers
13. AddHttpClient
14. Configure<IpRateLimitOptions> + AddInMemoryRateLimiting + AddSingleton<IRateLimitConfiguration>
--- Build ---
15. UseSwagger / UseSwaggerUI (Development only)
16. UseCors
17. UseHttpsRedirection
18. UseIpRateLimiting
19. UseAuthentication / UseAuthorization
20. MapControllers
21. UseStaticFiles
22. BrowserFetcher().DownloadAsync() (PuppeteerSharp)
23. SeedMockDataAsync (Development only)
24. Run
```

---

## 7. appsettings.json Template

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=JokerNutrition;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "BlobStorageSettings": {
    "ConnectionString": "UseDevelopmentStorage=true",
    "ContainerName": "joker-progress-photos"
  },
  "AzureStorage": {
    "ContainerName": "joker-nutrition",
    "MaxFileSizeInBytes": 10485760
  },
  "JwtSettings": {
    "SecretKey": "<generate-random-256-bit-key>",
    "Issuer": "https://localhost:7001",
    "Audience": "https://localhost:7001",
    "ExpiryInDays": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "",
    "Password": "",
    "FromEmail": "",
    "FromName": "Joker Nutrition",
    "SignUpBaseUrl": "https://localhost:5173/register",
    "SupportEmail": "support@jokernutrition.com"
  },
  "EmailTemplateSettings": {
    "LogoUrl": "",
    "LockIconUrl": ""
  },
  "AppSettings": {
    "ResetPasswordPageUrl": "http://localhost:5173/reset-password"
  },
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "HttpStatusCode": 429,
    "GeneralRules": [
      { "Endpoint": "post:/api/auth/forgot-password", "Period": "1h", "Limit": 3 },
      { "Endpoint": "post:/api/auth/login",            "Period": "1m", "Limit": 10 }
    ]
  }
}
```

---

## 8. Roles & Authorization

| Role | Description |
|---|---|
| `Admin` | Platform administrator — full access to all content, users, and admin libraries |
| `Coach` | Verified coach — manages athletes, builds workout templates, sets macro targets |
| `Athlete` | Client/athlete — logs meals, workouts, supplements; submits check-ins |

Seed all three roles in `SeedExtensions.cs` via `RoleManager<Role>`.

**Seed data required:**
- 3+ demo coaches, 10+ demo athletes with assigned coaches
- 50+ foods with accurate macro-per-100g data (Chicken Breast, Oats, Eggs, Rice, Tuna, Cottage Cheese, etc.)
- 5+ Joker-curated recipes with ingredients
- 30+ exercises across Chest/Back/Shoulders/Arms/Legs/Cardio categories with YouTube IDs
- 1 demo 6-day Push/Pull/Legs workout template
- Essential supplement schedules (Creatine, Multivitamins, Omega-3, Vitamin D3)
- Sample check-ins for the past 4 weeks

---

## 9. EF Core – DbContext & Fluent Mapping

```csharp
// Contexts/JokerNutritionContext.cs
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(JokerNutritionContext).Assembly);
    }
}
```

Each entity gets its own `ClassMapping/<EntityName>Mapping.cs` implementing `IEntityTypeConfiguration<TEntity>`.

**Performance indexes to add in ClassMappings:**
- `MealLogs.DailyDiaryId`
- `WorkoutLogs.AthleteId + Date`
- `ClientCheckIns.AthleteId + WeekOf`
- `SupplementLogs.SupplementScheduleId + Date`
- `Notifications.RecipientUserId + IsRead`

---

## 10. Coding Conventions to Follow

| Convention | Rule |
|---|---|
| Base classes | Prefix with underscore: `_BaseRepository<T>`, `_BaseService` |
| Interfaces | Prefix with `I`: `IAthleteService`, `_IBaseRepository<T>` |
| Mapper classes | Static methods only, `Map(entity)` → DTO, `Map(form)` → entity |
| Forms | Suffix with `Form`: `LogFoodForm`, `SubmitCheckInForm` |
| DTOs | Suffix with `Dto`: `AthleteDto`, `DailyDiaryDto`, `MacroSummaryDto` |
| Repositories | One repository per aggregate root |
| Services | One service per domain area, inject only repositories it owns |
| Exception handling | All exceptions caught in `ApiExceptionFilter`, log with Serilog |
| Pagination | All list endpoints accept `BasePaginationForm`, return `PagedResult<T>` |
| Async | All I/O operations are async (`async Task<T>`) |
| Logging | Use `ILogger` injected via constructor, log at Warning+ in production |

---

## 11. Step-by-Step Build Order

1. **Create solution** + three projects + add project references
2. **Install NuGet packages** for all three projects
3. **Data layer first**: Entities → Identity entities → DbContext → ClassMappings → BaseRepository → Concrete repositories → RepositoriesModule → Migrations
4. **Business layer**: Enums → Configuration POCOs → Helpers (JWT + MacroCalculator) → Security (AppPrincipal) → BaseService → Forms → DTOs → Mappers → Services → ServicesModule → Validations
5. **API layer**: Program.cs → Filters → ControllersModule → Controllers → Extensions (seed)
6. **Run initial migration**: `dotnet ef migrations add InitialCreate -p JokerNutrition.Data -s JokerNutrition.Api`
7. **Update database**: `dotnet ef database update -s JokerNutrition.Api`
8. **Seed mock data** in development mode
9. **Test** all endpoints via Swagger UI at `https://localhost:7001/swagger`

---

## 12. Joker Nutrition Domain-Specific Features

| Feature | Implementation |
|---|---|
| Macro calculator | `MacroCalculatorHelper.Calculate(food, quantityGrams, state)` — applies raw/cooked/dry conversion factors |
| Ramadan Mode | `MealType` enum includes `Suhoor`, `Iftar`, `PreWorkout`, `PostWorkout`. Toggle on `Athlete.RamadanModeEnabled`. Diary uses these labels when enabled. |
| Streak tracking | `WorkoutLogService.CompleteWorkout()` increments `Athlete.CurrentStreak` if yesterday was also logged; resets to 1 otherwise. Updates `Athlete.LongestStreak` if exceeded. |
| Live feed | `CoachHubService.GetLiveFeed()` queries `WorkoutLogs` ordered by `CompletedAt` descending across all coach's athletes — streams InProgress, Completed, Missed statuses. |
| Compliance alerts | `CoachHubService.GetComplianceRoster()` compares `DailyDiary` macro totals vs. `MacroTarget` — flags red when calorie total exceeds target by >5%. |
| Progress photo upload | `CheckInService.GetUploadUrl()` uses `BlobStorageService` to generate a 24-hour pre-signed Azure Blob PUT URL per photo angle. |
| Secure photo download | `CheckInService.GetPhotoUrls()` returns 24-hour expiring signed GET URLs — photos are never publicly accessible. |
| Check-in alert | On `CheckInService.SubmitCheckIn()` success, `NotificationService` creates a `CheckInSubmitted` notification for the assigned coach. |
| Bulk food import | `FoodService.BulkImportFoods(List<BulkFoodRow>)` parses CSV rows, validates macros sum to ≤ 900 kcal/100g, batch-inserts. Returns success/failure report. |
| Audit logging | `AuditLog` records: coach login, macro target changes, check-in photo access, and admin food/exercise deletions. |

---

> **Note**: All placeholder values (API keys, connection strings, secrets) must be placed in `appsettings.Development.json` which is `.gitignore`d. Never commit secrets to source control.
