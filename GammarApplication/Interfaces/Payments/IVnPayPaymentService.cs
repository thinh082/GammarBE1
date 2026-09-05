using GammarApplication.DTOs.Payments;

namespace GammarApplication.Interfaces.Payments;

public interface IVnPayPaymentService
{
    Task<CreateCourseOrderPaymentResultDto> CreateCourseOrderAsync(
        long userId,
        long courseId,
        string? clientIp,
        CancellationToken cancellationToken = default);

    Task<CourseOrderStatusDto?> GetCourseOrderAsync(
        long orderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseOrderStatusDto>> GetUserCourseOrdersAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<VnPayReturnResultDto> HandleReturnAsync(
        IReadOnlyDictionary<string, string> query,
        CancellationToken cancellationToken = default);

    Task<VnPayIpnResponseDto> HandleIpnAsync(
        IReadOnlyDictionary<string, string> query,
        CancellationToken cancellationToken = default);
}
