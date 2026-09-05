using GammarApplication.DTOs.MockExams;
using GammarApplication.Interfaces.MockExams;
using GammarApplication.Interfaces.Notifications;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Services.MockExams;

public class MockExamService : IMockExamService
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;

    public MockExamService(AppDbContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<IReadOnlyList<MockExamSummaryDto>> GetExamsAsync(string? level = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MockExams.AsNoTracking().Where(x => x.IsPublished);
        if (!string.IsNullOrWhiteSpace(level))
        {
            query = query.Where(x => x.Level.ToLower() == level.Trim().ToLower());
        }

        return await query
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Id)
            .Select(x => new MockExamSummaryDto(
                x.Id,
                x.Title,
                x.Level,
                x.DurationMinutes,
                x.PassingScore,
                x.TotalScore,
                x.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<MockExamDetailDto?> GetExamDetailAsync(long examId, CancellationToken cancellationToken = default)
    {
        var exam = await _dbContext.MockExams
            .AsNoTracking()
            .Include(x => x.Sections.OrderBy(s => s.OrderIndex))
                .ThenInclude(s => s.Questions.OrderBy(q => q.OrderIndex))
                    .ThenInclude(q => q.Options.OrderBy(o => o.OrderIndex))
            .FirstOrDefaultAsync(x => x.Id == examId && x.IsPublished, cancellationToken);

        if (exam is null)
        {
            return null;
        }

        var sectionDtos = exam.Sections.Select(s => new MockExamSectionDto(
            s.Id,
            s.Title,
            s.OrderIndex,
            s.TimeLimitMinutes,
            s.Questions.Select(q => new MockExamQuestionDto(
                q.Id,
                q.QuestionText,
                q.AudioUrl,
                q.ImageUrl,
                q.Points,
                q.OrderIndex,
                q.Options.Select(o => new MockExamOptionDto(
                    o.Id,
                    o.OptionText,
                    o.OrderIndex)).ToList()
            )).ToList()
        )).ToList();

        return new MockExamDetailDto(
            exam.Id,
            exam.Title,
            exam.Level,
            exam.DurationMinutes,
            exam.PassingScore,
            exam.TotalScore,
            exam.Description,
            sectionDtos);
    }

    public async Task<ExamResultDto> SubmitExamAttemptAsync(
        long userId,
        long examId,
        SubmitExamRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var exam = await _dbContext.MockExams
            .Include(x => x.Sections)
                .ThenInclude(s => s.Questions)
                    .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(x => x.Id == examId && x.IsPublished, cancellationToken);

        if (exam is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy đề thi thử với ID = {examId}");
        }

        var attempt = new UserMockExamAttempt(userId, examId);
        _dbContext.UserMockExamAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync(cancellationToken);

        int totalUserScore = 0;
        var answersToSave = new List<UserMockExamAnswer>();
        var userAnswersDict = request.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

        var allQuestions = exam.Sections.SelectMany(s => s.Questions).ToList();

        foreach (var question in allQuestions)
        {
            userAnswersDict.TryGetValue(question.Id, out var selectedOptionId);
            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            bool isCorrect = selectedOptionId.HasValue && correctOption != null && selectedOptionId.Value == correctOption.Id;

            int pointsAwarded = isCorrect ? question.Points : 0;
            totalUserScore += pointsAwarded;

            answersToSave.Add(new UserMockExamAnswer(
                attempt.Id,
                question.Id,
                selectedOptionId,
                isCorrect,
                pointsAwarded));
        }

        _dbContext.UserMockExamAnswers.AddRange(answersToSave);
        bool isPassed = totalUserScore >= exam.PassingScore;
        attempt.CompleteAttempt(totalUserScore, isPassed);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Auto trigger Notification
        string resultText = isPassed ? "ĐẠT (PASSED)" : "CHƯA ĐẠT (FAILED)";
        await _notificationService.SendNotificationAsync(
            userId,
            $"Kết quả thi thử JLPT {exam.Level}",
            $"Bạn đã hoàn thành đề thi \"{exam.Title}\" với kết quả {totalUserScore}/{exam.TotalScore} điểm ({resultText}).",
            "course",
            "/thi-thu",
            cancellationToken);

        return new ExamResultDto(
            attempt.Id,
            exam.Id,
            exam.Title,
            totalUserScore,
            exam.TotalScore,
            exam.PassingScore,
            isPassed,
            attempt.SubmittedAt ?? DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<ExamResultDto>> GetUserAttemptHistoryAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserMockExamAttempts
            .AsNoTracking()
            .Include(x => x.MockExam)
            .Where(x => x.UserId == userId && x.Status == "completed")
            .OrderByDescending(x => x.SubmittedAt)
            .Select(x => new ExamResultDto(
                x.Id,
                x.MockExamId,
                x.MockExam != null ? x.MockExam.Title : string.Empty,
                x.Score,
                x.MockExam != null ? x.MockExam.TotalScore : 180,
                x.MockExam != null ? x.MockExam.PassingScore : 90,
                x.IsPassed,
                x.SubmittedAt ?? x.StartedAt))
            .ToListAsync(cancellationToken);
    }
}
