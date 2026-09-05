namespace GammarDomain.Entities;

public class CourseOrder
{
    public long Id { get; private set; }
    public string OrderCode { get; private set; } = string.Empty;
    public long UserId { get; private set; }
    public long CourseId { get; private set; }
    public string Provider { get; private set; } = "vnpay";
    public string Status { get; private set; } = "pending";
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "VND";
    public string? OrderTitle { get; private set; }
    public string? OrderDescription { get; private set; }
    public string? VnpTxnRef { get; private set; }
    public string? VnpOrderInfo { get; private set; }
    public string? PaymentUrl { get; private set; }
    public string? ReturnUrl { get; private set; }
    public string? IpnUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? ExpiredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? Note { get; private set; }

    public User? User { get; private set; }
    public Course? Course { get; private set; }
    public ICollection<PaymentTransaction> PaymentTransactions { get; private set; } = [];

    private CourseOrder()
    {
    }

    public CourseOrder(
        string orderCode,
        long userId,
        long courseId,
        decimal amount,
        string currency,
        string provider,
        string? orderTitle,
        string? orderDescription,
        string? vnpTxnRef,
        string? vnpOrderInfo,
        string? paymentUrl,
        string? returnUrl,
        string? ipnUrl,
        DateTime? expiredAt,
        string? createdByIp)
    {
        OrderCode = orderCode;
        UserId = userId;
        CourseId = courseId;
        Amount = amount;
        Currency = currency;
        Provider = provider;
        OrderTitle = orderTitle;
        OrderDescription = orderDescription;
        VnpTxnRef = vnpTxnRef;
        VnpOrderInfo = vnpOrderInfo;
        PaymentUrl = paymentUrl;
        ReturnUrl = returnUrl;
        IpnUrl = ipnUrl;
        ExpiredAt = expiredAt;
        CreatedByIp = createdByIp;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePaymentUrl(string paymentUrl)
    {
        PaymentUrl = paymentUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkPaid(string? note = null)
    {
        Status = "paid";
        PaidAt = DateTime.UtcNow;
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string? note = null)
    {
        Status = "failed";
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCancelled(string? note = null)
    {
        Status = "cancelled";
        CancelledAt = DateTime.UtcNow;
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkExpired(string? note = null)
    {
        Status = "expired";
        ExpiredAt = DateTime.UtcNow;
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefunded(string? note = null)
    {
        Status = "refunded";
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }
}
