namespace GammarApplication.Exceptions;

public sealed class PaymentOperationException : Exception
{
    public int StatusCode { get; }

    public PaymentOperationException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}
