namespace JokerNutrition.Business.DTOs.Coach;

public class CoachFeedbackNoteDto
{
    public int Id { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string CoachName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = "General";
    public string? WeekOf { get; set; }
}
