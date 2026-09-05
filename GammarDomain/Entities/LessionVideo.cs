namespace GammarDomain.Entities;

public class LessionVideo
{
    public long Id { get; private set; }
    public long LessonId { get; private set; }
    public string? Title { get; private set; }
    public string VideoUrl { get; private set; } = string.Empty;
    public string? VideoProvider { get; private set; }
    public int? DurationSeconds { get; private set; }
    public string? TranscriptText { get; private set; }
    public string? SubtitleUrl { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Lesson? Lesson { get; private set; }

    private LessionVideo()
    {
    }
}
