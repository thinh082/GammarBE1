using GammarApplication.DTOs.Admin;

namespace GammarApplication.Interfaces.Admin;

public interface IAdminManagementService
{
    Task<AdminManagementOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<AdminUserCourseManagementOverviewDto> GetUserCourseManagementAsync(
        string? keyword = null,
        string? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
