using GammarApplication.DTOs.MockExams;

namespace GammarApplication.Interfaces.MockExams;

public interface IMockExamService
{
    Task<IReadOnlyList<MockExamSummaryDto>> GetExamsAsync(string? level = null, CancellationToken cancellationToken = default);
    Task<MockExamDetailDto?> GetExamDetailAsync(long examId, CancellationToken cancellationToken = default);
    Task<ExamResultDto> SubmitExamAttemptAsync(long userId, long examId, SubmitExamRequestDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExamResultDto>> GetUserAttemptHistoryAsync(long userId, CancellationToken cancellationToken = default);
}
