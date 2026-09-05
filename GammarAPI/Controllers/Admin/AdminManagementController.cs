using GammarApplication.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/management")]
public class AdminManagementController : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService;

    public AdminManagementController(IAdminManagementService adminManagementService)
    {
        _adminManagementService = adminManagementService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await _adminManagementService.GetOverviewAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("user-courses")]
    public async Task<IActionResult> GetUserCourses(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminManagementService.GetUserCourseManagementAsync(
            keyword,
            status,
            page,
            pageSize,
            cancellationToken);
        return Ok(result);
    }
}
