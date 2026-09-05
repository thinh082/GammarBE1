namespace GammarApplication.DTOs.Admin;

public sealed record AdminUserCourseManagementItemDto(
    long UserId,
    string UserEmail,
    string? UserFullName,
    long CourseId,
    string CourseCode,
    string CourseTitle,
    string? CourseSlug,
    string Status,
    decimal CourseProgressPercent,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    DateTime? ExpiredAt,
    DateTime UpdatedAt,
    int CompletedLessons,
    int TotalLessons,
    long? CurrentLessonId,
    string? CurrentLessonTitle,
    long? LastVideoId,
    string? LastVideoTitle,
    int? LastPositionSeconds);

public sealed record AdminUserCourseManagementOverviewDto(
    int TotalCount,
    IReadOnlyList<AdminUserCourseManagementItemDto> Items);
