using JokerNutrition.Data.Enums;

namespace JokerNutrition.Business.Forms.Foods;

public class SearchFoodsForm
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public FoodState? State { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
