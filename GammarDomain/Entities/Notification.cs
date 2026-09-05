namespace GammarDomain.Entities;

public class Notification
{
    public long Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Type { get; private set; } = "system";
    public string? TargetUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<UserNotification> UserNotifications { get; private set; } = [];

    private Notification()
    {
    }

    public Notification(string title, string content, string type = "system", string? targetUrl = null)
    {
        Title = title.Trim();
        Content = content.Trim();
        Type = type.Trim();
        TargetUrl = targetUrl?.Trim();
        CreatedAt = DateTime.UtcNow;
    }
}
