using JokerNutrition.Business.DTOs.CheckIns;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class CheckInMapper
{
    public static CheckInDto Map(ClientCheckIn entity, List<CheckInPhotoDto>? photos = null)
    {
        return new CheckInDto
        {
            Id = entity.Id,
            AthleteId = entity.AthleteId,
            AthleteFullName = entity.Athlete is not null
                ? $"{entity.Athlete.User.FirstName} {entity.Athlete.User.LastName}"
                : string.Empty,
            WeekOf = entity.WeekOf.ToString("yyyy-MM-dd"),
            SubmittedAt = entity.SubmittedAt.ToString("o"),
            WeightKg = entity.WeightKg,
            WaistCm = entity.WaistCm,
            ChestCm = entity.ChestCm,
            ThighCm = entity.ThighCm,
            SleepQuality = entity.SleepQuality,
            EnergyLevel = entity.EnergyLevel,
            GutHealth = entity.GutHealth,
            TrainingStress = entity.TrainingStress,
            CoachNotes = entity.CoachNotes,
            CoachReviewedAt = entity.CoachReviewedAt?.ToString("o"),
            Photos = photos ?? new List<CheckInPhotoDto>()
        };
    }

    public static CheckInPhotoDto MapPhoto(CheckInPhoto photo, string signedUrl)
    {
        return new CheckInPhotoDto
        {
            Id = photo.Id,
            Angle = photo.Angle.ToString(),
            SignedDownloadUrl = signedUrl,
            UploadedAt = photo.UploadedAt.ToString("o")
        };
    }

    public static PendingCheckInDto MapPending(Athlete athlete, ClientCheckIn? lastCheckIn)
    {
        int daysSince = -1;
        string? lastWeekOf = null;

        if (lastCheckIn is not null)
        {
            lastWeekOf = lastCheckIn.WeekOf.ToString("yyyy-MM-dd");
            daysSince = (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - lastCheckIn.WeekOf.DayNumber);
        }

        return new PendingCheckInDto
        {
            AthleteId = athlete.Id,
            AthleteFullName = athlete.User is not null
                ? $"{athlete.User.FirstName} {athlete.User.LastName}"
                : string.Empty,
            ProfilePictureUrl = athlete.User?.ProfilePictureUrl,
            LastCheckInWeekOf = lastWeekOf,
            DaysSinceLastCheckIn = daysSince
        };
    }
}
