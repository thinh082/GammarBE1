namespace GammarAPI.DTOs.Courses;

public sealed record LessonVideoDto(
    long Id,
    string? Title,
    string VideoUrl,
    string? VideoProvider,
    int? DurationSeconds,
    string? TranscriptText,
    string? SubtitleUrl,
    int SortOrder);

public sealed record LessonTextDto(
    long Id,
    string? Title,
    string? ContentText,
    string? ContentHtml,
    string? AttachmentUrl,
    int SortOrder);

public sealed record LessonQuizOptionDto(
    long Id,
    string? OptionLabel,
    string OptionText,
    int SortOrder);

public sealed record LessonQuizQuestionDto(
    long Id,
    string QuestionText,
    string? ExplanationText,
    int SortOrder,
    IReadOnlyList<LessonQuizOptionDto> Options);

public sealed record LessonQuizDto(
    long Id,
    string Title,
    string? Description,
    decimal PassingScore,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    IReadOnlyList<LessonQuizQuestionDto> Questions);

public sealed record LessonDetailDto(
    long Id,
    long CourseId,
    string? Code,
    string Title,
    string LessonKind,
    string? ShortDescription,
    int SortOrder,
    bool IsPreview,
    IReadOnlyList<LessonVideoDto> Videos,
    IReadOnlyList<LessonTextDto> Texts,
    LessonQuizDto? Quiz);
