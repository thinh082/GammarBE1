namespace GammarDomain.Entities;

public class UserNotification
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long NotificationId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User? User { get; private set; }
    public Notification? Notification { get; private set; }

    private UserNotification()
    {
    }

    public UserNotification(long userId, long notificationId)
    {
        UserId = userId;
        NotificationId = notificationId;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}
