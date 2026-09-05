namespace GammarAPI.DTOs.Courses;

public sealed record LessonDiscussionAuthorDto(
    long UserId,
    string DisplayName,
    string? AvatarUrl,
    string? Email);

public sealed record LessonDiscussionItemDto(
    long Id,
    long LessonId,
    long UserId,
    long? ParentId,
    string Content,
    int LikeCount,
    int ReplyCount,
    bool IsEdited,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsLikedByCurrentUser,
    LessonDiscussionAuthorDto Author,
    IReadOnlyList<LessonDiscussionItemDto> Replies);

public sealed record CreateLessonDiscussionRequest(
    long UserId,
    string Content);

public sealed record ToggleLessonDiscussionLikeRequest(
    long UserId);
