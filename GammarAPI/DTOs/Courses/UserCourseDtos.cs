namespace GammarAPI.DTOs.Courses;

public sealed record UserCourseDto(
    long Id,
    long UserId,
    long CourseId,
    string Status,
    decimal ProgressPercent,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime? ExpiredAt,
    CourseListItemDto Course);
