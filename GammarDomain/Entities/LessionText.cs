namespace GammarDomain.Entities;

public class LessionText
{
    public long Id { get; private set; }
    public long LessonId { get; private set; }
    public string? Title { get; private set; }
    public string? ContentText { get; private set; }
    public string? ContentHtml { get; private set; }
    public string? AttachmentUrl { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Lesson? Lesson { get; private set; }

    private LessionText()
    {
    }
}
