namespace GammarAPI.Requests.Payments;

public sealed class CreateCourseOrderPaymentRequest
{
    public long UserId { get; init; }
    public long CourseId { get; init; }
}
