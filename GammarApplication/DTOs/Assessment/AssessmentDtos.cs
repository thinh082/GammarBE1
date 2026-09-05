namespace GammarApplication.DTOs.Assessment;

public record AssessmentOptionDto(
    long Id,
    string OptionText);

public record AssessmentQuestionDto(
    long Id,
    string QuestionText,
    string Level,
    int OrderIndex,
    IReadOnlyList<AssessmentOptionDto> Options);

public record UserAssessmentAnswerDto(
    long QuestionId,
    long SelectedOptionId);

public record SubmitAssessmentRequestDto(
    IReadOnlyList<UserAssessmentAnswerDto> Answers);

public record AssessmentResultDto(
    string RecommendedLevel,
    int TotalScore,
    int MaxScore,
    DateTime TakenAt);
