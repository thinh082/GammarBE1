namespace GammarDomain.Entities;

public class LessonQuiz
{
    public long Id { get; private set; }
    public long LessonId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal? PassingScore { get; private set; }
    public int? TimeLimitMinutes { get; private set; }
    public int? MaxAttempts { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Lesson? Lesson { get; private set; }
    public ICollection<LessonQuizQuestion> Questions { get; private set; } = [];

    private LessonQuiz()
    {
    }
}
