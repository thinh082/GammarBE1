using GammarApplication.DTOs.Admin;
using GammarApplication.Interfaces.Admin;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Services.Admin;

public class AdminManagementService : IAdminManagementService
{
    private readonly AppDbContext _context;

    public AdminManagementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminManagementOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var publishedCourses = await _context.Courses.CountAsync(x => x.IsPublished, cancellationToken);
        var activeVocabularies = await _context.Vocabularies.CountAsync(x => x.IsActive, cancellationToken);
        var activeCategories = await _context.ProductCategories.CountAsync(x => x.IsActive, cancellationToken);

        var cards = new List<AdminMetricCardDto>
        {
            new("users", "Nguoi dung", totalUsers.ToString(), "Tong tai khoan dang co trong he thong."),
            new("courses", "Khoa hoc da xuat ban", publishedCourses.ToString(), "Co the dua thang vao dashboard quan ly khoa hoc."),
            new("vocabularies", "Tu vung dang hoat dong", activeVocabularies.ToString(), "Du lieu cho kho tu vung va hoc lieu."),
            new("categories", "Danh muc dang hoat dong", activeCategories.ToString(), "Khung quan ly nhom khoa hoc va san pham."),
        };

        var modules = new List<AdminModuleDescriptorDto>
        {
            new(
                "courses",
                "Quan ly khoa hoc",
                "CRUD khoa hoc va doc danh sach bai hoc trong tung khoa.",
                new List<AdminEndpointDescriptorDto>
                {
                    new("Danh sach khoa hoc", "GET", "/api/Courses", "Lay danh sach khoa hoc public hien tai."),
                    new("Tao khoa hoc", "POST", "/api/Courses", "Them khoa hoc moi tu khu admin."),
                    new("Cap nhat khoa hoc", "PUT", "/api/Courses/{courseId}", "Sua thong tin khoa hoc."),
                    new("An khoa hoc", "DELETE", "/api/Courses/{courseId}", "Unpublish khoa hoc thay vi xoa cung."),
                }),
            new(
                "categories",
                "Quan ly danh muc",
                "Quan ly product category de phan loai khoa hoc.",
                new List<AdminEndpointDescriptorDto>
                {
                    new("Danh sach danh muc", "GET", "/api/ProductCategories", "Lay tat ca danh muc dang hoat dong."),
                    new("Tao danh muc", "POST", "/api/ProductCategories", "Them danh muc moi."),
                    new("Cap nhat danh muc", "PUT", "/api/ProductCategories/{id}", "Sua ten, mo ta va thu tu."),
                    new("Ngung su dung danh muc", "DELETE", "/api/ProductCategories/{id}", "Deactivate danh muc."),
                }),
            new(
                "vocabularies",
                "Quan ly tu vung",
                "Quan ly kho tu vung, muc do va vi du.",
                new List<AdminEndpointDescriptorDto>
                {
                    new("Danh sach tu vung", "GET", "/api/Vocabularies", "Lay kho tu vung co bo loc."),
                    new("Tao tu vung", "POST", "/api/Vocabularies", "Them muc tu vung moi."),
                    new("Cap nhat tu vung", "PUT", "/api/Vocabularies/{id}", "Sua noi dung tu vung."),
                    new("Ngung hien thi tu vung", "DELETE", "/api/Vocabularies/{id}", "Deactivate tu vung."),
                }),
            new(
                "user-courses",
                "Quan ly khoa hoc cua hoc vien",
                "Gan khoa hoc cho user va theo doi tien do hoc.",
                new List<AdminEndpointDescriptorDto>
                {
                    new("Danh sach khoa hoc da gan", "GET", "/api/Users/{userId}/courses", "Lay toan bo khoa hoc cua mot user."),
                    new("Chi tiet user course", "GET", "/api/Users/{userId}/courses/{courseId}", "Lay trang thai hoc cua user theo khoa."),
                    new("Gan khoa hoc cho user", "POST", "/api/Users/{userId}/courses", "Assign khoa hoc cho hoc vien."),
                    new("Cap nhat tien do", "PATCH", "/api/Users/{userId}/courses/{courseId}/progress", "Dong bo phan tram tien do."),
                }),
        };

        return new AdminManagementOverviewDto(DateTime.UtcNow, cards, modules);
    }

    public async Task<AdminUserCourseManagementOverviewDto> GetUserCourseManagementAsync(
        string? keyword = null,
        string? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var normalizedKeyword = keyword?.Trim().ToLowerInvariant();
        var normalizedStatus = status?.Trim().ToLowerInvariant();

        var query = _context.UserCourses
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedStatus))
        {
            query = query.Where(x => x.Status.ToLower() == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(normalizedKeyword))
        {
            query = query.Where(x =>
                (x.User != null && (
                    x.User.Email.ToLower().Contains(normalizedKeyword) ||
                    _context.Profiles.Any(profile =>
                        profile.UserId == x.UserId &&
                        profile.FullName != null &&
                        profile.FullName.ToLower().Contains(normalizedKeyword)))) ||
                (x.Course != null && (
                    x.Course.Title.ToLower().Contains(normalizedKeyword) ||
                    x.Course.Code.ToLower().Contains(normalizedKeyword))));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var userCourses = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                UserCourse = x,
                FullName = _context.Profiles
                    .Where(profile => profile.UserId == x.UserId)
                    .Select(profile => profile.FullName)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var courseIds = userCourses.Select(x => x.UserCourse.CourseId).Distinct().ToList();
        var userIds = userCourses.Select(x => x.UserCourse.UserId).Distinct().ToList();

        var totalLessonsByCourse = await _context.Lessons
            .AsNoTracking()
            .Where(x => courseIds.Contains(x.CourseId) && x.IsActive)
            .GroupBy(x => x.CourseId)
            .Select(group => new { CourseId = group.Key, TotalLessons = group.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.TotalLessons, cancellationToken);

        var progressItems = await _context.UserLessonProgresses
            .AsNoTracking()
            .Include(x => x.Lesson)
            .Include(x => x.LastVideo)
            .Where(x =>
                userIds.Contains(x.UserId) &&
                x.Lesson != null &&
                courseIds.Contains(x.Lesson.CourseId))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var progressByEnrollment = progressItems
            .Where(x => x.Lesson is not null)
            .GroupBy(x => (x.UserId, x.Lesson!.CourseId))
            .ToDictionary(group => group.Key, group => group.ToList());

        var items = userCourses.Select(item =>
        {
            var userCourse = item.UserCourse;
            var enrollmentKey = (userCourse.UserId, userCourse.CourseId);
            progressByEnrollment.TryGetValue(enrollmentKey, out var enrollmentProgresses);
            var completedLessons = enrollmentProgresses?.Count(x => string.Equals(x.Status, "completed", StringComparison.OrdinalIgnoreCase)) ?? 0;
            var latestProgress = enrollmentProgresses?.FirstOrDefault();
            var totalLessons = totalLessonsByCourse.TryGetValue(userCourse.CourseId, out var lessonsCount) ? lessonsCount : 0;

            return new AdminUserCourseManagementItemDto(
                userCourse.UserId,
                userCourse.User?.Email ?? string.Empty,
                item.FullName,
                userCourse.CourseId,
                userCourse.Course?.Code ?? string.Empty,
                userCourse.Course?.Title ?? string.Empty,
                userCourse.Course?.Slug,
                userCourse.Status,
                userCourse.ProgressPercent,
                userCourse.StartedAt,
                userCourse.CompletedAt,
                userCourse.ExpiredAt,
                userCourse.UpdatedAt,
                completedLessons,
                totalLessons,
                latestProgress?.LessonId,
                latestProgress?.Lesson?.Title,
                latestProgress?.LastVideoId,
                latestProgress?.LastVideo?.Title,
                latestProgress?.LastPositionSeconds);
        }).ToList();

        return new AdminUserCourseManagementOverviewDto(totalCount, items);
    }
}
