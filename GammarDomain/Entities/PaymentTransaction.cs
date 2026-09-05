namespace GammarDomain.Entities;

public class PaymentTransaction
{
    public long Id { get; private set; }
    public long OrderId { get; private set; }
    public string Provider { get; private set; } = "vnpay";
    public string TransactionType { get; private set; } = "pay";
    public string Status { get; private set; } = "initiated";
    public string? RequestId { get; private set; }
    public string? TransactionRef { get; private set; }
    public string? ProviderTransactionNo { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "VND";
    public string? BankCode { get; private set; }
    public string? BankTranNo { get; private set; }
    public string? CardType { get; private set; }
    public string? ResponseCode { get; private set; }
    public string? TransactionStatusCode { get; private set; }
    public string? PayDate { get; private set; }
    public string? SecureHash { get; private set; }
    public string? RawQuery { get; private set; }
    public string? RawRequest { get; private set; }
    public string? RawResponse { get; private set; }
    public string? RawIpn { get; private set; }
    public string? RawReturn { get; private set; }
    public DateTime? IpnReceivedAt { get; private set; }
    public DateTime? ReturnReceivedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? Note { get; private set; }

    public CourseOrder? Order { get; private set; }

    private PaymentTransaction()
    {
    }

    public PaymentTransaction(
        long orderId,
        string provider,
        string transactionType,
        string status,
        string? requestId,
        string? transactionRef,
        decimal amount,
        string currency,
        string? secureHash,
        string? rawQuery,
        string? rawRequest,
        string? rawResponse,
        string? note = null)
    {
        OrderId = orderId;
        Provider = provider;
        TransactionType = transactionType;
        Status = status;
        RequestId = requestId;
        TransactionRef = transactionRef;
        Amount = amount;
        Currency = currency;
        SecureHash = secureHash;
        RawQuery = rawQuery;
        RawRequest = rawRequest;
        RawResponse = rawResponse;
        Note = note;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordReturn(
        string? responseCode,
        string? transactionStatusCode,
        string? providerTransactionNo,
        string? bankCode,
        string? bankTranNo,
        string? cardType,
        string? payDate,
        string? secureHash,
        string? rawQuery,
        string? rawReturn)
    {
        ResponseCode = responseCode;
        TransactionStatusCode = transactionStatusCode;
        ProviderTransactionNo = providerTransactionNo;
        BankCode = bankCode;
        BankTranNo = bankTranNo;
        CardType = cardType;
        PayDate = payDate;
        SecureHash = secureHash;
        RawQuery = rawQuery;
        RawReturn = rawReturn;
        ReturnReceivedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(string status, string? note = null)
    {
        Status = status;
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordIpn(
        string status,
        string? responseCode,
        string? transactionStatusCode,
        string? providerTransactionNo,
        string? bankCode,
        string? bankTranNo,
        string? cardType,
        string? payDate,
        string? secureHash,
        string? rawQuery,
        string? rawIpn,
        string? note = null)
    {
        Status = status;
        ResponseCode = responseCode;
        TransactionStatusCode = transactionStatusCode;
        ProviderTransactionNo = providerTransactionNo;
        BankCode = bankCode;
        BankTranNo = bankTranNo;
        CardType = cardType;
        PayDate = payDate;
        SecureHash = secureHash;
        RawQuery = rawQuery;
        RawIpn = rawIpn;
        IpnReceivedAt = DateTime.UtcNow;
        Note = note ?? Note;
        UpdatedAt = DateTime.UtcNow;
    }
}
