namespace GammarAPI.DTOs.Courses;

public sealed record CreateCourseRequest(
    long ProductCategoryId,
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

public sealed record UpdateCourseRequest(
    long ProductCategoryId,
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
