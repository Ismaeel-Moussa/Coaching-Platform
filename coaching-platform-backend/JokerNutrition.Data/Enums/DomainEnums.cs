namespace JokerNutrition.Data.Enums;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack,
    Suhoor,
    Iftar,
    PreWorkout,
    PostWorkout
}


public enum RecipeCategory
{
    MuscleBuilding,
    FatLoss,
    Custom
}

public enum MuscleGroup
{
    Chest,
    Back,
    Shoulders,
    Arms,
    Legs,
    Cardio,
    Core
}

public enum ExerciseSection
{
    WarmUp,
    Main,
    CoolDown
}

public enum WorkoutStatus
{
    InProgress,
    Completed,
    Missed
}

public enum SupplementType
{
    Essential,
    Optional
}

public enum ContentStatus
{
    Draft,
    InReview,
    Published,
    Archived
}

public enum FoodPreparationState
{
    Unspecified,
    Raw,
    Cooked,
    Drained
}

public enum IngredientUnit
{
    Gram,
    Milliliter,
    Piece,
    Tablespoon,
    Teaspoon,
    Cup,
    Scoop
}

public enum SeedImportStatus
{
    Running,
    Succeeded,
    Failed
}

public enum PhotoAngle
{
    Front,
    Side,
    Back
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Expired,
    Revoked
}

public enum NotificationType
{
    CheckInSubmitted,
    WorkoutCompleted,
    CoachNote,
    MacroAlert,
    InvitationAccepted
}
