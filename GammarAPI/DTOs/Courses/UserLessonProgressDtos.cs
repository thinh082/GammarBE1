namespace GammarAPI.DTOs.Courses;

public sealed record UserLessonProgressDto(
    long Id,
    long UserId,
    long LessonId,
    string Status,
    decimal ProgressPercent,
    long? LastVideoId,
    int? LastPositionSeconds,
    DateTime? CompletedAt,
    DateTime UpdatedAt);

public sealed record UserCourseLessonProgressSummaryDto(
    long CourseId,
    decimal CourseProgressPercent,
    int CompletedLessons,
    int TotalLessons,
    long? CurrentLessonId,
    IReadOnlyList<UserLessonProgressDto> LessonProgresses);

public sealed record UpdateUserLessonProgressRequest(
    decimal? ProgressPercent,
    string? Status,
    long? LastVideoId,
    int? LastPositionSeconds,
    bool MarkCompleted = false);

public sealed record UpdateUserLessonProgressResultDto(
    decimal CourseProgressPercent,
    int CompletedLessons,
    int TotalLessons,
    long? CurrentLessonId,
    UserLessonProgressDto LessonProgress);
