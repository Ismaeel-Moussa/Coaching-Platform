using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Data.Entities;
using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Mappers;

public static class CoachHubMapper
{
    // ─── Live Feed ────────────────────────────────────────────────────

    public static LiveFeedItemDto MapLiveFeedItem(WorkoutLog log) => new()
    {
        AthleteId = log.Athlete.Id,
        AthleteName = $"{log.Athlete.User.FirstName} {log.Athlete.User.LastName}",
        AthleteAvatarUrl = log.Athlete.User.ProfilePictureUrl,
        WorkoutDayLabel = log.Day.DayLabel,
        Status = log.Status.ToString(),
        CompletedAt = log.CompletedAt,
        Date = log.Date
    };

    // ─── Compliance ───────────────────────────────────────────────────

    public static ComplianceItemDto MapComplianceItem(
        Athlete athlete,
        DailyDiary? diary,
        MacroTarget? target)
    {
        var targetCalories = target?.TargetCalories ?? 0m;
        var consumed = diary != null
            ? diary.MealLogs.Sum(m => m.Calories)
            : 0m;

        var consumedProtein = diary != null ? diary.MealLogs.Sum(m => m.Protein) : 0m;
        var consumedCarbs = diary != null ? diary.MealLogs.Sum(m => m.Carbs) : 0m;
        var consumedFat = diary != null ? diary.MealLogs.Sum(m => m.Fat) : 0m;

        // Over-target = consumed > target by more than 5%
        var isOver = targetCalories > 0 && consumed > targetCalories * 1.05m;

        // Compliance % = how close to target (capped at 100)
        double compliancePct = 0;
        if (targetCalories > 0)
        {
            compliancePct = Math.Min(100.0, (double)(consumed / targetCalories) * 100.0);
        }

        return new ComplianceItemDto
        {
            AthleteId = athlete.Id,
            AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}",
            AthleteAvatarUrl = athlete.User.ProfilePictureUrl,
            TargetCalories = targetCalories,
            ConsumedCalories = consumed,
            TargetProtein = target?.TargetProtein ?? 0m,
            ConsumedProtein = consumedProtein,
            TargetCarbs = target?.TargetCarbs ?? 0m,
            ConsumedCarbs = consumedCarbs,
            TargetFat = target?.TargetFat ?? 0m,
            ConsumedFat = consumedFat,
            IsOverCalorieTarget = isOver,
            CompliancePercent = Math.Round(compliancePct, 1)
        };
    }

    // ─── Roster ───────────────────────────────────────────────────────

    public static RosterItemDto MapRosterItem(
        Athlete athlete,
        ClientProgram? activeProgram,
        ClientCheckIn? lastCheckIn,
        double compliancePct)
    {
        var hasRecentCheckIn = lastCheckIn != null &&
                               lastCheckIn.SubmittedAt >= DateTime.UtcNow.AddDays(-7);

        var isCompliantAlert = compliancePct > 105.0 || compliancePct < 40.0;

        var status = !hasRecentCheckIn ? "NoRecentCheckIn"
            : isCompliantAlert ? "ComplianceAlert"
            : "Active";

        return new RosterItemDto
        {
            AthleteId = athlete.Id,
            AthleteName = $"{athlete.User.FirstName} {athlete.User.LastName}",
            AthleteAvatarUrl = athlete.User.ProfilePictureUrl,
            ActiveProgramName = activeProgram?.WorkoutTemplate?.Name,
            MacroCompliancePercent = Math.Round(compliancePct, 1),
            LastCheckInDate = lastCheckIn?.SubmittedAt,
            Status = status
        };
    }

    // ─── Feedback Note ────────────────────────────────────────────────

    public static CoachFeedbackNoteDto MapFeedbackNote(CoachFeedbackNote note) => new()
    {
        Id = note.Id,
        NoteText = note.NoteText,
        CoachName = $"{note.Coach.User.FirstName} {note.Coach.User.LastName}",
        CreatedAt = note.CreatedAt
    };

    // ─── Weight History ───────────────────────────────────────────────

    public static WeightHistoryPointDto MapWeightPoint(ClientCheckIn checkIn) => new()
    {
        WeekOf = checkIn.WeekOf,
        WeightKg = checkIn.WeightKg
    };
}
