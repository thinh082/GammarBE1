using System.Globalization;
using GammarApplication.DTOs.Admin;
using GammarApplication.Interfaces.Admin;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GammarInfrastructure.Services.Admin;

public class AdminReportsService : IAdminReportsService
{
    private readonly AppDbContext _context;

    public AdminReportsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminReportsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var assignedCourses = await _context.UserCourses.CountAsync(cancellationToken);
        var completedAssignments = await _context.UserCourses.CountAsync(x => x.Status == "completed", cancellationToken);
        var favoriteVocabularies = await _context.UserFavoriteVocabularies.CountAsync(cancellationToken);
        var averageProgress = assignedCourses == 0
            ? 0m
            : await _context.UserCourses.AverageAsync(x => x.ProgressPercent, cancellationToken);

        var cards = new List<AdminMetricCardDto>
        {
            new("assigned-courses", "Luot gan khoa hoc", assignedCourses.ToString(), "So ban ghi user_course dang ton tai."),
            new("completed-assignments", "Luot hoan thanh", completedAssignments.ToString(), "Hoc vien da dat 100 phan tram tien do."),
            new("favorite-vocabularies", "Luot yeu thich tu vung", favoriteVocabularies.ToString(), "Chi so tham khao cho muc do quan tam hoc lieu."),
            new("average-progress", "Tien do trung binh", FormatPercent(averageProgress), "Trung binh tien do tren toan bo user course."),
        };

        var sections = new List<AdminReportSectionDto>
        {
            await GetRevenueSummaryReportAsync(cancellationToken),
            await GetCoursePublicationReportAsync(cancellationToken),
            await GetVocabularyInventoryReportAsync(cancellationToken),
            await GetUserCourseProgressReportAsync(cancellationToken),
        };

        return new AdminReportsOverviewDto(DateTime.UtcNow, cards, sections);
    }

    public async Task<AdminReportSectionDto> GetRevenueSummaryReportAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _context.CourseOrders
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalOrders = orders.Count;
        var paidOrders = orders.Where(x => x.Status == "paid").ToList();
        var pendingOrders = orders.Where(x => x.Status == "pending").ToList();
        var failedOrders = orders.Where(x => x.Status == "failed" || x.Status == "cancelled" || x.Status == "expired").ToList();

        var paidRevenue = paidOrders.Sum(x => x.Amount);
        var pendingRevenue = pendingOrders.Sum(x => x.Amount);
        var paymentSuccessRate = totalOrders == 0
            ? 0m
            : Math.Round((decimal)paidOrders.Count / totalOrders * 100m, 2);

        var topCourseSummaries = await (
            from order in _context.CourseOrders.AsNoTracking()
            join course in _context.Courses.AsNoTracking() on order.CourseId equals course.Id
            where order.Status == "paid"
            group new { order, course } by new { course.Id, course.Title } into grouped
            orderby grouped.Sum(x => x.order.Amount) descending, grouped.Count() descending
            select new
            {
                grouped.Key.Title,
                Revenue = grouped.Sum(x => x.order.Amount),
            })
            .Take(3)
            .ToListAsync(cancellationToken);

        var topCourses = topCourseSummaries.Count == 0
            ? "Chua co"
            : string.Join(", ", topCourseSummaries.Select(x => $"{x.Title}: {FormatCurrency(x.Revenue)}"));

        return new AdminReportSectionDto(
            "revenue-summary",
            "Bao cao doanh thu",
            "Tong quan ngan gon ve dong tien thanh toan va hieu qua chot don khoa hoc.",
            new List<AdminReportRowDto>
            {
                new("Doanh thu da thu", FormatCurrency(paidRevenue), "Tong gia tri don hang da thanh toan thanh cong."),
                new("Gia tri cho thanh toan", FormatCurrency(pendingRevenue), "Tong gia tri cac don dang cho xu ly."),
                new("Don hang thanh cong", paidOrders.Count.ToString(), "So don co trang thai paid."),
                new("Don hang cho xu ly", pendingOrders.Count.ToString(), "So don dang o trang thai pending."),
                new("Don hang loi hoac huy", failedOrders.Count.ToString(), "Gom failed, cancelled va expired."),
                new("Ti le thanh cong", FormatPercent(paymentSuccessRate), "Ty le paid tren tong so don hang."),
                new("Top khoa hoc doanh thu", topCourses, "3 khoa hoc co doanh thu paid cao nhat hien tai."),
            });
    }

    public async Task<AdminReportSectionDto> GetCoursePublicationReportAsync(CancellationToken cancellationToken = default)
    {
        var totalCourses = await _context.Courses.CountAsync(cancellationToken);
        var publishedCourses = await _context.Courses.CountAsync(x => x.IsPublished, cancellationToken);
        var draftCourses = totalCourses - publishedCourses;
        var freeCourses = await _context.Courses.CountAsync(x => x.IsPublished && x.IsFree, cancellationToken);
        var hotCourses = await _context.Courses.CountAsync(x => x.IsPublished && x.IsHot, cancellationToken);

        return new AdminReportSectionDto(
            "course-publication",
            "Bao cao xuat ban khoa hoc",
            "Khung bao cao nhanh cho tinh trang khoa hoc tren he thong.",
            new List<AdminReportRowDto>
            {
                new("Tong khoa hoc", totalCourses.ToString(), "Bao gom ca khoa dang an va da xuat ban."),
                new("Khoa hoc da xuat ban", publishedCourses.ToString(), "Dang duoc frontend public su dung."),
                new("Khoa hoc chua xuat ban", draftCourses.ToString(), "Can ra soat truoc khi dua len public."),
                new("Khoa hoc mien phi", freeCourses.ToString(), "So khoa hoc dang mo free."),
                new("Khoa hoc noi bat", hotCourses.ToString(), "Danh dau de uu tien hien thi."),
            });
    }

    public async Task<AdminReportSectionDto> GetVocabularyInventoryReportAsync(CancellationToken cancellationToken = default)
    {
        var vocabularies = await _context.Vocabularies.ToListAsync(cancellationToken);
        var total = vocabularies.Count;
        var active = vocabularies.Count(x => x.IsActive);
        var inactive = total - active;
        var jlptLevels = vocabularies
            .Where(x => !string.IsNullOrWhiteSpace(x.LevelCode))
            .GroupBy(x => x.LevelCode!)
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}: {x.Count()}")
            .ToList();

        return new AdminReportSectionDto(
            "vocabulary-inventory",
            "Bao cao kho tu vung",
            "Tong quan so luong va phan bo tu vung.",
            new List<AdminReportRowDto>
            {
                new("Tong tu vung", total.ToString(), "Tong ban ghi trong bang vocabulary."),
                new("Tu vung dang hoat dong", active.ToString(), "Co the hien thi ra kho tu vung."),
                new("Tu vung tam an", inactive.ToString(), "Dang deactivate hoac can ra soat."),
                new("Phan bo theo level", jlptLevels.Count == 0 ? "Chua co" : string.Join(", ", jlptLevels), "Giup lap ke hoach hoc lieu theo N5-N1."),
            });
    }

    public async Task<AdminReportSectionDto> GetUserCourseProgressReportAsync(CancellationToken cancellationToken = default)
    {
        var assignments = await _context.UserCourses.ToListAsync(cancellationToken);
        var total = assignments.Count;
        var active = assignments.Count(x => x.Status == "active");
        var completed = assignments.Count(x => x.Status == "completed");
        var averageProgress = total == 0 ? 0m : assignments.Average(x => x.ProgressPercent);

        return new AdminReportSectionDto(
            "user-course-progress",
            "Bao cao tien do hoc tap",
            "Tong hop phan bo tien do hoc vien theo user_course.",
            new List<AdminReportRowDto>
            {
                new("Tong luot gan khoa hoc", total.ToString(), "So ban ghi user_course hien co."),
                new("Dang hoc", active.ToString(), "Trang thai active."),
                new("Da hoan thanh", completed.ToString(), "Trang thai completed."),
                new("Tien do trung binh", FormatPercent(averageProgress), "Trung binh tien do tren moi user_course."),
            });
    }

    private static string FormatPercent(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }

    private static string FormatCurrency(decimal value)
    {
        return value.ToString("#,0", CultureInfo.InvariantCulture) + " VND";
    }
}
