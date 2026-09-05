using GammarApplication.DTOs.Assessment;
using GammarApplication.Interfaces.Assessment;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;

    public AssessmentController(IAssessmentService assessmentService)
    {
        _assessmentService = assessmentService;
    }

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions(CancellationToken cancellationToken)
    {
        var questions = await _assessmentService.GetQuestionsAsync(cancellationToken);
        return Ok(questions);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAssessment(
        [FromQuery] long userId,
        [FromBody] SubmitAssessmentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return BadRequest("UserId là bắt buộc.");
        }

        var result = await _assessmentService.SubmitAssessmentAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("users/{userId:long}/latest")]
    public async Task<IActionResult> GetLatestResult(long userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return BadRequest("UserId là bắt buộc.");
        }

        var result = await _assessmentService.GetLatestUserResultAsync(userId, cancellationToken);
        if (result is null)
        {
            return NotFound("Chưa có kết quả đánh giá cho học viên này.");
        }

        return Ok(result);
    }
}
