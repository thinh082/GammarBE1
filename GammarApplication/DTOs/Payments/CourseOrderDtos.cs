namespace GammarApplication.DTOs.Payments;

public sealed record CreateCourseOrderPaymentResultDto(
    long OrderId,
    string OrderCode,
    string Status,
    decimal Amount,
    string Currency,
    string PaymentUrl,
    DateTime? ExpiresAt);

public sealed record CourseOrderStatusDto(
    long OrderId,
    string OrderCode,
    long UserId,
    long CourseId,
    string CourseTitle,
    string? CourseSlug,
    string Status,
    decimal Amount,
    string Currency,
    DateTime CreatedAt,
    DateTime? PaidAt);

public sealed record VnPayReturnResultDto(
    long? OrderId,
    string? OrderCode,
    string ResultCode,
    string ResultMessage,
    string RedirectUrl);

public sealed record VnPayIpnResponseDto(
    string RspCode,
    string Message);
