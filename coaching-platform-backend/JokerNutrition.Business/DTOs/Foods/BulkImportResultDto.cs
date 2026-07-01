namespace JokerNutrition.Business.DTOs.Foods;

public class BulkImportResultDto
{
    public int InsertedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
