using GammarInfrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/users/{userId:long}/progress")]
public class UserProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public UserProgressController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserProgress(long userId, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
        if (!userExists)
        {
            return NotFound(new { message = "User not found" });
        }

        // 1. Get enrolled courses
        var enrolledCourses = await _context.UserCourses
            .Where(uc => uc.UserId == userId)
            .Include(uc => uc.Course)
            .ToListAsync(cancellationToken);

        var enrolledCourseIds = enrolledCourses.Select(uc => uc.CourseId).ToList();

        // 2. Completed lessons count
        var completedLessons = await _context.UserLessonProgresses
            .Where(ulp => ulp.UserId == userId && ulp.Status == "completed")
            .Select(ulp => ulp.LessonId)
            .ToListAsync(cancellationToken);

        // 3. Build course progress items
        var courseProgresses = new List<CourseProgressDto>();
        var totalLessons = 0;
        var totalCompletedLessons = 0;

        foreach (var uc in enrolledCourses)
        {
            var courseId = uc.CourseId;
            var lessonsInCourse = await _context.Lessons
                .Where(l => l.CourseId == courseId && l.IsActive)
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);

            var totalInCourse = lessonsInCourse.Count;
            var completedInCourse = lessonsInCourse.Count(id => completedLessons.Contains(id));

            totalLessons += totalInCourse;
            totalCompletedLessons += completedInCourse;

            courseProgresses.Add(new CourseProgressDto
            {
                CourseId = courseId,
                CourseTitle = uc.Course?.Title ?? "Khóa học",
                LevelCode = uc.Course?.LevelCode ?? "",
                CompletedLessons = completedInCourse,
                TotalLessons = totalInCourse,
                ProgressPercent = totalInCourse > 0 ? Math.Round((double)completedInCourse * 100 / totalInCourse, 1) : 0
            });
        }

        // 4. Saved vocabulary count
        var savedVocabulariesCount = await _context.UserFavoriteVocabularies
            .CountAsync(ufv => ufv.UserId == userId, cancellationToken);

        // 5. Mock exam attempts and score trend
        var examAttempts = await _context.UserMockExamAttempts
            .Where(uma => uma.UserId == userId && uma.Status == "completed")
            .Include(uma => uma.MockExam)
            .OrderBy(uma => uma.SubmittedAt)
            .ToListAsync(cancellationToken);

        var averageExamScore = examAttempts.Count > 0 
            ? Math.Round(examAttempts.Average(a => a.Score), 1) 
            : 0;

        var examScoreTrend = examAttempts.Select(a => new ExamAttemptProgressDto
        {
            AttemptId = a.Id,
            ExamTitle = a.MockExam?.Title ?? "Thi thử",
            Level = a.MockExam?.Level ?? "",
            Score = a.Score,
            SubmittedAt = a.SubmittedAt ?? DateTime.UtcNow
        }).ToList();

        // 6. Recent activities (Combined from Lessons, Vocabs, Exams)
        var recentLessons = await _context.UserLessonProgresses
            .Where(ulp => ulp.UserId == userId && ulp.Status == "completed")
            .Include(ulp => ulp.Lesson)
            .OrderByDescending(ulp => ulp.CompletedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentVocabs = await _context.UserFavoriteVocabularies
            .Where(ufv => ufv.UserId == userId)
            .Include(ufv => ufv.Vocabulary)
            .OrderByDescending(ufv => ufv.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentAttempts = await _context.UserMockExamAttempts
            .Where(uma => uma.UserId == userId && uma.Status == "completed")
            .Include(uma => uma.MockExam)
            .OrderByDescending(uma => uma.SubmittedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var activities = new List<RecentActivityDto>();

        foreach (var item in recentLessons)
        {
            activities.Add(new RecentActivityDto
            {
                Description = $"Bạn đã hoàn thành bài học \"{item.Lesson?.Title ?? "Bài học"}\".",
                CreatedAt = item.CompletedAt ?? item.UpdatedAt,
                Type = "lesson"
            });
        }

        foreach (var item in recentVocabs)
        {
            var displayWord = string.IsNullOrEmpty(item.Vocabulary?.Kanji)
                ? (item.Vocabulary?.Kana ?? "Từ mới")
                : item.Vocabulary.Kanji;
            activities.Add(new RecentActivityDto
            {
                Description = $"Bạn đã lưu từ vựng \"{displayWord}\" ({item.Vocabulary?.MeaningVi ?? ""}) vào sổ tay.",
                CreatedAt = item.CreatedAt,
                Type = "vocabulary"
            });
        }

        foreach (var item in recentAttempts)
        {
            activities.Add(new RecentActivityDto
            {
                Description = $"Bạn đã nộp bài thi thử \"{item.MockExam?.Title ?? "Đề thi"}\" đạt {item.Score}/180 điểm.",
                CreatedAt = item.SubmittedAt ?? DateTime.UtcNow,
                Type = "exam"
            });
        }

        var recentActivitiesList = activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .ToList();

        return Ok(new UserProgressDetailDto
        {
            CompletedLessons = totalCompletedLessons,
            TotalLessons = totalLessons,
            SavedVocabulariesCount = savedVocabulariesCount,
            AverageExamScore = averageExamScore,
            CourseProgresses = courseProgresses,
            ExamScoreTrend = examScoreTrend,
            RecentActivities = recentActivitiesList
        });
    }
}

public class UserProgressDetailDto
{
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public int SavedVocabulariesCount { get; set; }
    public double AverageExamScore { get; set; }
    public List<CourseProgressDto> CourseProgresses { get; set; } = [];
    public List<ExamAttemptProgressDto> ExamScoreTrend { get; set; } = [];
    public List<RecentActivityDto> RecentActivities { get; set; } = [];
}

public class CourseProgressDto
{
    public long CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string LevelCode { get; set; } = string.Empty;
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public double ProgressPercent { get; set; }
}

public class ExamAttemptProgressDto
{
    public long AttemptId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class RecentActivityDto
{
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = string.Empty; // "lesson", "vocabulary", "exam"
}
