using System;

namespace JokerNutrition.Data.Entities;

public class FavoriteFood
{
    public int Id { get; set; }
    public int AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public int FoodId { get; set; }
    public Food Food { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
