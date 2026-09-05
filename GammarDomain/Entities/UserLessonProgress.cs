namespace GammarDomain.Entities;

public class UserLessonProgress
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long LessonId { get; private set; }
    public string Status { get; private set; } = "not_started";
    public decimal ProgressPercent { get; private set; }
    public long? LastVideoId { get; private set; }
    public int? LastPositionSeconds { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public Lesson? Lesson { get; private set; }
    public LessionVideo? LastVideo { get; private set; }

    private UserLessonProgress()
    {
    }

    public UserLessonProgress(long userId, long lessonId)
    {
        UserId = userId;
        LessonId = lessonId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Sync(
        decimal? progressPercent,
        string? status,
        long? lastVideoId,
        int? lastPositionSeconds,
        bool markCompleted)
    {
        if (progressPercent.HasValue)
        {
            ProgressPercent = Math.Clamp(progressPercent.Value, 0m, 100m);
        }

        if (lastVideoId.HasValue)
        {
            LastVideoId = lastVideoId;
        }

        if (lastPositionSeconds.HasValue)
        {
            LastPositionSeconds = Math.Max(0, lastPositionSeconds.Value);
        }

        if (markCompleted || string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase) || ProgressPercent >= 100m)
        {
            Status = "completed";
            ProgressPercent = 100m;
            CompletedAt ??= DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        if (string.Equals(Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            ProgressPercent = 100m;
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        if (string.Equals(status, "not_started", StringComparison.OrdinalIgnoreCase) && ProgressPercent <= 0m && (LastPositionSeconds ?? 0) <= 0)
        {
            Status = "not_started";
            CompletedAt = null;
            UpdatedAt = DateTime.UtcNow;
            return;
        }

        if (string.Equals(status, "learning", StringComparison.OrdinalIgnoreCase) || ProgressPercent > 0m || (LastPositionSeconds ?? 0) > 0 || LastVideoId.HasValue)
        {
            Status = "learning";
            CompletedAt = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
