using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Workouts;

public class AssignTemplateForm
{
    [Required, MinLength(1)]
    public List<int> AthleteIds { get; set; } = new();
}
