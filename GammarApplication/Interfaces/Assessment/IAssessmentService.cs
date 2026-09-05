using GammarApplication.DTOs.Assessment;

namespace GammarApplication.Interfaces.Assessment;

public interface IAssessmentService
{
    Task<IReadOnlyList<AssessmentQuestionDto>> GetQuestionsAsync(CancellationToken cancellationToken = default);
    Task<AssessmentResultDto> SubmitAssessmentAsync(long userId, SubmitAssessmentRequestDto request, CancellationToken cancellationToken = default);
    Task<AssessmentResultDto?> GetLatestUserResultAsync(long userId, CancellationToken cancellationToken = default);
}
