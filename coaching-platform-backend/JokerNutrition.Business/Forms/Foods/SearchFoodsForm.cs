namespace JokerNutrition.Business.Forms.Foods;

public class SearchFoodsForm
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
