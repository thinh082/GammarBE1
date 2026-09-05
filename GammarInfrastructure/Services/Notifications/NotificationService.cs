using GammarApplication.DTOs.Notifications;
using GammarApplication.Interfaces.Notifications;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;

    public NotificationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserNotifications
            .AsNoTracking()
            .Include(x => x.Notification)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto(
                x.NotificationId,
                x.Notification != null ? x.Notification.Title : string.Empty,
                x.Notification != null ? x.Notification.Content : string.Empty,
                x.Notification != null ? x.Notification.Type : "system",
                x.Notification != null ? x.Notification.TargetUrl : null,
                x.IsRead,
                x.ReadAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(long userId, long notificationId, CancellationToken cancellationToken = default)
    {
        var userNotif = await _dbContext.UserNotifications
            .FirstOrDefaultAsync(x => x.UserId == userId && x.NotificationId == notificationId, cancellationToken);

        if (userNotif is null)
        {
            return false;
        }

        userNotif.MarkAsRead();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default)
    {
        var unreadList = await _dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (unreadList.Count == 0)
        {
            return true;
        }

        foreach (var unread in unreadList)
        {
            unread.MarkAsRead();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SendNotificationAsync(long userId, string title, string content, string type = "system", string? targetUrl = null, CancellationToken cancellationToken = default)
    {
        var notif = new Notification(title, content, type, targetUrl);
        _dbContext.Notifications.Add(notif);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var userNotif = new UserNotification(userId, notif.Id);
        _dbContext.UserNotifications.Add(userNotif);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
