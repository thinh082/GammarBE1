using GammarApplication.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers.Admin;

[ApiController]
[Route("api/admin/reports")]
public class AdminReportsController : ControllerBase
{
    private readonly IAdminReportsService _adminReportsService;

    public AdminReportsController(IAdminReportsService adminReportsService)
    {
        _adminReportsService = adminReportsService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var result = await _adminReportsService.GetOverviewAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("revenue-summary")]
    public async Task<IActionResult> GetRevenueSummaryReport(CancellationToken cancellationToken)
    {
        var result = await _adminReportsService.GetRevenueSummaryReportAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("course-publication")]
    public async Task<IActionResult> GetCoursePublicationReport(CancellationToken cancellationToken)
    {
        var result = await _adminReportsService.GetCoursePublicationReportAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("vocabulary-inventory")]
    public async Task<IActionResult> GetVocabularyInventoryReport(CancellationToken cancellationToken)
    {
        var result = await _adminReportsService.GetVocabularyInventoryReportAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("user-course-progress")]
    public async Task<IActionResult> GetUserCourseProgressReport(CancellationToken cancellationToken)
    {
        var result = await _adminReportsService.GetUserCourseProgressReportAsync(cancellationToken);
        return Ok(result);
    }
}
