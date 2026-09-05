namespace GammarDomain.Entities;

public class LessonQuizQuestion
{
    public long Id { get; private set; }
    public long LessonQuizId { get; private set; }
    public string QuestionText { get; private set; } = string.Empty;
    public string? ExplanationText { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public LessonQuiz? LessonQuiz { get; private set; }
    public ICollection<LessonQuizOption> Options { get; private set; } = [];

    private LessonQuizQuestion()
    {
    }
}
