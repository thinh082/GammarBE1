using GammarApplication.DTOs.Admin;

namespace GammarApplication.Interfaces.Admin;

public interface IAdminReportsService
{
    Task<AdminReportsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
    Task<AdminReportSectionDto> GetRevenueSummaryReportAsync(CancellationToken cancellationToken = default);
    Task<AdminReportSectionDto> GetCoursePublicationReportAsync(CancellationToken cancellationToken = default);
    Task<AdminReportSectionDto> GetVocabularyInventoryReportAsync(CancellationToken cancellationToken = default);
    Task<AdminReportSectionDto> GetUserCourseProgressReportAsync(CancellationToken cancellationToken = default);
}
