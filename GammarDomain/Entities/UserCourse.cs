namespace GammarDomain.Entities;

public class UserCourse
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long CourseId { get; private set; }
    public string Status { get; private set; } = "active";
    public decimal ProgressPercent { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public User? User { get; private set; }
    public Course? Course { get; private set; }

    private UserCourse()
    {
    }

    public UserCourse(long userId, long courseId, string status = "active")
    {
        UserId = userId;
        CourseId = courseId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkStarted()
    {
        if (StartedAt is null)
        {
            StartedAt = DateTime.UtcNow;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(decimal progressPercent)
    {
        ProgressPercent = progressPercent;
        if (progressPercent > 0 && StartedAt is null)
        {
            StartedAt = DateTime.UtcNow;
        }

        if (progressPercent >= 100)
        {
            CompletedAt = DateTime.UtcNow;
            Status = "completed";
        }
        else if (Status == "completed")
        {
            Status = "active";
            CompletedAt = null;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
