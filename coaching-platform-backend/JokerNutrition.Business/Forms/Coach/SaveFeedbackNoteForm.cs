using System.ComponentModel.DataAnnotations;

namespace JokerNutrition.Business.Forms.Coach;

public class SaveFeedbackNoteForm
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string NoteText { get; set; } = string.Empty;
}
