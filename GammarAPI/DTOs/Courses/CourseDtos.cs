namespace GammarAPI.DTOs.Courses;

public sealed record CourseListItemDto(
    long Id,
    long ProductCategoryId,
    string ProductCategoryCode,
    string ProductCategoryName,
    string Code,
    string Slug,
    string Title,
    string? ShortDescription,
    string? ThumbnailUrl,
    string? LevelCode,
    int? DurationMonths,
    decimal Price,
    decimal? OriginalPrice,
    string Currency,
    bool IsFree,
    bool IsHot,
    bool IsPublished,
    int SortOrder);

public sealed record CourseDetailDto(
    long Id,
    long ProductCategoryId,
    string ProductCategoryCode,
    string ProductCategoryName,
    string Code,
    string Slug,
    string Title,
    string? ShortDescription,
    string? ThumbnailUrl,
    string? LevelCode,
    int? DurationMonths,
    decimal Price,
    decimal? OriginalPrice,
    string Currency,
    bool IsFree,
    bool IsHot,
    bool IsPublished,
    int SortOrder,
    int LessonCount);

public sealed record CourseLessonItemDto(
    long Id,
    string? Code,
    string Title,
    string LessonKind,
    string? ShortDescription,
    int SortOrder,
    bool IsPreview,
    bool IsActive);
