using GammarApplication.DTOs.Assessment;
using GammarApplication.Interfaces.Assessment;
using GammarApplication.Interfaces.Notifications;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Services.Assessment;

public class AssessmentService : IAssessmentService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public AssessmentService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<AssessmentQuestionDto>> GetQuestionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.AssessmentQuestions
            .AsNoTracking()
            .Include(x => x.Options)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new AssessmentQuestionDto(
                x.Id,
                x.QuestionText,
                x.Level,
                x.OrderIndex,
                x.Options.Select(o => new AssessmentOptionDto(o.Id, o.OptionText)).ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<AssessmentResultDto> SubmitAssessmentAsync(
        long userId,
        SubmitAssessmentRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var questions = await _dbContext.AssessmentQuestions
            .Include(x => x.Options)
            .ToListAsync(cancellationToken);

        int totalCorrect = 0;
        int maxScore = questions.Count;
        var userAnswersDict = request.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

        var correctLevelCount = new Dictionary<string, int>
        {
            ["N5"] = 0,
            ["N4"] = 0,
            ["N3"] = 0,
            ["N2"] = 0,
            ["N1"] = 0
        };

        foreach (var q in questions)
        {
            var correctOption = q.Options.FirstOrDefault(o => o.IsCorrect);
            if (userAnswersDict.TryGetValue(q.Id, out var selectedId) && correctOption != null && selectedId == correctOption.Id)
            {
                totalCorrect++;
                if (correctLevelCount.ContainsKey(q.Level))
                {
                    correctLevelCount[q.Level]++;
                }
            }
        }

        // Determine recommended level based on highest level passed
        string recommendedLevel = "N5";
        if (correctLevelCount["N1"] >= 1) recommendedLevel = "N1";
        else if (correctLevelCount["N2"] >= 2) recommendedLevel = "N2";
        else if (correctLevelCount["N3"] >= 2) recommendedLevel = "N3";
        else if (correctLevelCount["N4"] >= 2) recommendedLevel = "N4";
        else recommendedLevel = "N5";

        var result = new UserAssessmentResult(userId, recommendedLevel, totalCorrect, maxScore);
        _dbContext.UserAssessmentResults.Add(result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Auto trigger Notification
        await _notificationService.SendNotificationAsync(
            userId,
            "Kết quả Đánh giá Trình độ Đầu vào",
            $"Bạn đã hoàn thành bài test với kết quả {totalCorrect}/{maxScore} câu đúng. Trình độ phù hợp đề xuất cho bạn là {recommendedLevel}.",
            "system",
            "/auth/assessment",
            cancellationToken);

        return new AssessmentResultDto(
            result.RecommendedLevel,
            result.TotalScore,
            result.MaxScore,
            result.TakenAt);
    }

    public async Task<AssessmentResultDto?> GetLatestUserResultAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAssessmentResults
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.TakenAt)
            .Select(x => new AssessmentResultDto(
                x.RecommendedLevel,
                x.TotalScore,
                x.MaxScore,
                x.TakenAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
