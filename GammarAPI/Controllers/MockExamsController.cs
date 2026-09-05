using GammarApplication.DTOs.MockExams;
using GammarApplication.Interfaces.MockExams;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MockExamsController : ControllerBase
{
    private readonly IMockExamService _mockExamService;

    public MockExamsController(IMockExamService mockExamService)
    {
        _mockExamService = mockExamService;
    }

    [HttpGet]
    public async Task<IActionResult> GetExams([FromQuery] string? level, CancellationToken cancellationToken)
    {
        var exams = await _mockExamService.GetExamsAsync(level, cancellationToken);
        return Ok(exams);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetExamDetail(long id, CancellationToken cancellationToken)
    {
        var exam = await _mockExamService.GetExamDetailAsync(id, cancellationToken);
        if (exam is null)
        {
            return NotFound("Không tìm thấy đề thi thử.");
        }

        return Ok(exam);
    }

    [HttpPost("{id:long}/submit")]
    public async Task<IActionResult> SubmitExam(
        long id,
        [FromQuery] long userId,
        [FromBody] SubmitExamRequestDto request,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return BadRequest("UserId là bắt buộc.");
        }

        try
        {
            var result = await _mockExamService.SubmitExamAttemptAsync(userId, id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("users/{userId:long}/history")]
    public async Task<IActionResult> GetUserHistory(long userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return BadRequest("UserId là bắt buộc.");
        }

        var history = await _mockExamService.GetUserAttemptHistoryAsync(userId, cancellationToken);
        return Ok(history);
    }
}
