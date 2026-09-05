namespace GammarApplication.DTOs.MockExams;

public record MockExamOptionDto(
    long Id,
    string OptionText,
    int OrderIndex);

public record MockExamQuestionDto(
    long Id,
    string QuestionText,
    string? AudioUrl,
    string? ImageUrl,
    int Points,
    int OrderIndex,
    IReadOnlyList<MockExamOptionDto> Options);

public record MockExamSectionDto(
    long Id,
    string Title,
    int OrderIndex,
    int? TimeLimitMinutes,
    IReadOnlyList<MockExamQuestionDto> Questions);

public record MockExamDetailDto(
    long Id,
    string Title,
    string Level,
    int DurationMinutes,
    int PassingScore,
    int TotalScore,
    string? Description,
    IReadOnlyList<MockExamSectionDto> Sections);

public record MockExamSummaryDto(
    long Id,
    string Title,
    string Level,
    int DurationMinutes,
    int PassingScore,
    int TotalScore,
    string? Description);

public record UserAnswerSubmissionDto(
    long QuestionId,
    long? SelectedOptionId);

public record SubmitExamRequestDto(
    IReadOnlyList<UserAnswerSubmissionDto> Answers);

public record ExamResultDto(
    long AttemptId,
    long MockExamId,
    string ExamTitle,
    int Score,
    int TotalScore,
    int PassingScore,
    bool IsPassed,
    DateTime SubmittedAt);
