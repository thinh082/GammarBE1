using GammarApplication.Interfaces.Notifications;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("users/{userId:long}")]
    public async Task<IActionResult> GetUserNotifications(long userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return BadRequest("UserId là bắt buộc.");
        }

        var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpPut("users/{userId:long}/{notificationId:long}/read")]
    public async Task<IActionResult> MarkAsRead(long userId, long notificationId, CancellationToken cancellationToken)
    {
        var success = await _notificationService.MarkAsReadAsync(userId, notificationId, cancellationToken);
        if (!success)
        {
            return NotFound("Không tìm thấy thông báo.");
        }

        return Ok(new { message = "Đã đánh dấu đã đọc" });
    }

    [HttpPut("users/{userId:long}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(long userId, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new { message = "Đã đánh dấu đọc tất cả" });
    }
}
