using GammarApplication.DTOs.Admin;

namespace GammarApplication.Interfaces.Admin;

public interface IAdminStatisticsService
{
    Task<AdminStatisticsOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
}
