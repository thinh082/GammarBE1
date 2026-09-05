namespace GammarAPI.DTOs.Courses;

public sealed record SubmitLessonQuizAnswerRequest(
    long QuestionId,
    long SelectedOptionId);

public sealed record SubmitLessonQuizRequest(
    IReadOnlyList<SubmitLessonQuizAnswerRequest> Answers);

public sealed record QuizSubmissionQuestionResultDto(
    long QuestionId,
    long? SelectedOptionId,
    long CorrectOptionId,
    bool IsCorrect,
    string? ExplanationText);

public sealed record QuizSubmissionResultDto(
    decimal Score,
    int CorrectCount,
    int TotalQuestions,
    bool Passed,
    IReadOnlyList<QuizSubmissionQuestionResultDto> QuestionResults);
