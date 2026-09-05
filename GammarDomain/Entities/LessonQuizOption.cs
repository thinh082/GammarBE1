namespace GammarDomain.Entities;

public class LessonQuizOption
{
    public long Id { get; private set; }
    public long LessonQuizQuestionId { get; private set; }
    public string? OptionLabel { get; private set; }
    public string OptionText { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public LessonQuizQuestion? LessonQuizQuestion { get; private set; }

    private LessonQuizOption()
    {
    }
}
