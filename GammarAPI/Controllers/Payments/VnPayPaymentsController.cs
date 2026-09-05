using GammarAPI.Requests.Payments;
using GammarApplication.Exceptions;
using GammarApplication.Interfaces.Payments;
using Microsoft.AspNetCore.Mvc;

namespace GammarAPI.Controllers.Payments;

[ApiController]
[Route("api/payments")]
public sealed class VnPayPaymentsController : ControllerBase
{
    private readonly IVnPayPaymentService _vnPayPaymentService;

    public VnPayPaymentsController(IVnPayPaymentService vnPayPaymentService)
    {
        _vnPayPaymentService = vnPayPaymentService;
    }

    [HttpPost("vnpay/course-orders")]
    public async Task<IActionResult> CreateCourseOrder(
        [FromBody] CreateCourseOrderPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.UserId <= 0 || request.CourseId <= 0)
        {
            return BadRequest("UserId và CourseId là bắt buộc.");
        }

        try
        {
            var result = await _vnPayPaymentService.CreateCourseOrderAsync(
                request.UserId,
                request.CourseId,
                GetClientIp(),
                cancellationToken);
            return Ok(result);
        }
        catch (PaymentOperationException ex)
        {
            return StatusCode(ex.StatusCode, ex.Message);
        }
    }

    [HttpGet("course-orders/{orderId:long}")]
    public async Task<IActionResult> GetCourseOrder(long orderId, CancellationToken cancellationToken)
    {
        var order = await _vnPayPaymentService.GetCourseOrderAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound("Không tìm thấy đơn hàng.");
        }

        return Ok(order);
    }

    [HttpGet("users/{userId:long}/course-orders")]
    public async Task<IActionResult> GetUserCourseOrders(long userId, CancellationToken cancellationToken)
    {
        if (userId <= 0)
        {
            return BadRequest("UserId là bắt buộc.");
        }

        var orders = await _vnPayPaymentService.GetUserCourseOrdersAsync(userId, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("vnpay/return")]
    public async Task<IActionResult> HandleReturn(CancellationToken cancellationToken)
    {
        var result = await _vnPayPaymentService.HandleReturnAsync(ToDictionary(Request.Query), cancellationToken);
        return Redirect(result.RedirectUrl);
    }

    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> HandleIpn(CancellationToken cancellationToken)
    {
        var result = await _vnPayPaymentService.HandleIpnAsync(ToDictionary(Request.Query), cancellationToken);
        return Ok(result);
    }

    private string? GetClientIp()
    {
        var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            return forwarded.Split(',')[0].Trim();
        }

        var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp.Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static IReadOnlyDictionary<string, string> ToDictionary(IQueryCollection query)
    {
        return query.ToDictionary(
            item => item.Key,
            item => item.Value.ToString(),
            StringComparer.Ordinal);
    }
}
