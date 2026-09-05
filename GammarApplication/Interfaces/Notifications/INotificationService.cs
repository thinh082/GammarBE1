using GammarApplication.DTOs.Notifications;

namespace GammarApplication.Interfaces.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(long userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(long userId, long notificationId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default);
    Task SendNotificationAsync(long userId, string title, string content, string type = "system", string? targetUrl = null, CancellationToken cancellationToken = default);
}
