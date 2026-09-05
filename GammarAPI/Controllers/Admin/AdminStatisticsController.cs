using GammarApplication.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/statistics")]
public class AdminStatisticsController : ControllerBase
{
    private readonly IAdminStatisticsService _adminStatisticsService;

    public AdminStatisticsController(IAdminStatisticsService adminStatisticsService)
    {
        _adminStatisticsService = adminStatisticsService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await _adminStatisticsService.GetOverviewAsync(cancellationToken);
        return Ok(result);
    }
}
