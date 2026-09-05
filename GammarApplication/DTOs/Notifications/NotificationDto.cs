namespace GammarApplication.DTOs.Notifications;

public record NotificationDto(
    long Id,
    string Title,
    string Content,
    string Type,
    string? TargetUrl,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);
