using JokerNutrition.Business.DTOs.Athletes;
using JokerNutrition.Data.Entities;

namespace JokerNutrition.Business.Mappers;

public static class MacroTargetMapper
{
    public static MacroTargetDto Map(MacroTarget target, string coachName) => new()
    {
        Id = target.Id,
        TargetCalories = target.TargetCalories,
        TargetProtein = target.TargetProtein,
        TargetCarbs = target.TargetCarbs,
        TargetFat = target.TargetFat,
        WaterLitersTarget = target.WaterLitersTarget,
        StepsTarget = target.StepsTarget,
        SetAt = target.SetAt,
        SetByCoachName = coachName
    };
}
