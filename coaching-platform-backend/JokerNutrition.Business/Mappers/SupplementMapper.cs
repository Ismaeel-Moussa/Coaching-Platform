using JokerNutrition.Business.DTOs.Supplements;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class SupplementMapper
{
    public static SupplementDto Map(SupplementSchedule schedule, SupplementLog? todayLog) => new()
    {
        Id = schedule.Id,
        Name = schedule.Name,
        Type = schedule.Type.ToString(),
        Dosage = schedule.Dosage,
        Notes = schedule.Notes,
        IsTakenToday = todayLog?.IsTaken ?? false,
        TakenAt = todayLog?.TakenAt
    };
}
