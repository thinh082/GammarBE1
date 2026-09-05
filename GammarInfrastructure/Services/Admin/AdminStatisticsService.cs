using GammarApplication.DTOs.Admin;
using GammarApplication.Interfaces.Admin;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Services.Admin;

public class AdminStatisticsService : IAdminStatisticsService
{
    private readonly AppDbContext _context;

    public AdminStatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminStatisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var activeUsers = await _context.Users.CountAsync(x => x.Status == "active", cancellationToken);
        var totalCourses = await _context.Courses.CountAsync(cancellationToken);
        var publishedCourses = await _context.Courses.CountAsync(x => x.IsPublished, cancellationToken);
        var totalLessons = await _context.Lessons.CountAsync(cancellationToken);
        var activeLessons = await _context.Lessons.CountAsync(x => x.IsActive, cancellationToken);
        var totalVocabularies = await _context.Vocabularies.CountAsync(cancellationToken);
        var activeVocabularies = await _context.Vocabularies.CountAsync(x => x.IsActive, cancellationToken);
        var assignedCourses = await _context.UserCourses.CountAsync(cancellationToken);
        var completedAssignments = await _context.UserCourses.CountAsync(x => x.Status == "completed", cancellationToken);

        var items = new List<AdminStatisticItemDto>
        {
            new("active-user-rate", "Ti le user active", CalculateRate(activeUsers, totalUsers), "%", "Ty le tai khoan dang o trang thai active."),
            new("published-course-rate", "Ti le khoa hoc xuat ban", CalculateRate(publishedCourses, totalCourses), "%", "Ty le khoa hoc dang duoc public."),
            new("active-lesson-rate", "Ti le bai hoc hoat dong", CalculateRate(activeLessons, totalLessons), "%", "Ty le lesson co the hien thi."),
            new("active-vocabulary-rate", "Ti le tu vung hoat dong", CalculateRate(activeVocabularies, totalVocabularies), "%", "Ty le tu vung dang duoc mo."),
            new("course-completion-rate", "Ti le hoan thanh khoa hoc", CalculateRate(completedAssignments, assignedCourses), "%", "Ty le user_course da hoan thanh."),
        };

        return new AdminStatisticsOverviewDto(DateTime.UtcNow, items);
    }

    private static decimal CalculateRate(int numerator, int denominator)
    {
        if (denominator <= 0)
        {
            return 0m;
        }

        return Math.Round((decimal)numerator * 100 / denominator, 2);
    }
}
