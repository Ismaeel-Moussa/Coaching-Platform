using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JokerNutrition.Business.Forms.Foods;

public class BulkImportFoodsForm
{
    [Required]
    public IFormFile CsvFile { get; set; } = null!;
}
