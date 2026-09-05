namespace GammarDomain.Entities;

public class Lesson
{
    public long Id { get; private set; }
    public long CourseId { get; private set; }
    public string? Code { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string LessonKind { get; private set; } = "mixed";
    public string? ShortDescription { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPreview { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Course? Course { get; private set; }
    public ICollection<LessionVideo> Videos { get; private set; } = [];
    public ICollection<LessionText> Texts { get; private set; } = [];
    public LessonQuiz? Quiz { get; private set; }

    private Lesson()
    {
    }
}
