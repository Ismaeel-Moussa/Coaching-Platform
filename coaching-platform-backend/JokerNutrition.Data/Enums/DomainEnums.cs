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
