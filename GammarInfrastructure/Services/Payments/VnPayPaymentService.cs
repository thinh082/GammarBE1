using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GammarApplication.DTOs.Payments;
using GammarApplication.Exceptions;
using GammarApplication.Interfaces.Notifications;
using GammarApplication.Interfaces.Payments;
using GammarDomain.Entities;
using GammarInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GammarInfrastructure.Services.Payments;

public sealed class VnPayPaymentService : IVnPayPaymentService
{
    private const string ProviderName = "vnpay";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _dbContext;
    private readonly ILogger<VnPayPaymentService> _logger;
    private readonly INotificationService _notificationService;
    private readonly string _tmnCode;
    private readonly string _hashSecret;
    private readonly string _paymentUrl;
    private readonly string _returnUrl;
    private readonly string _ipnUrl;
    private readonly string _version;
    private readonly string _command;
    private readonly string _currency;
    private readonly string _locale;
    private readonly string _orderType;
    private readonly string _frontendResultUrl;
    private readonly TimeZoneInfo _vietnamTimeZone;

    public VnPayPaymentService(
        AppDbContext dbContext,
        IConfiguration configuration,
        ILogger<VnPayPaymentService> logger,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _notificationService = notificationService;
        _tmnCode = GetRequiredSetting(configuration, "VNPAY_TMN_CODE");
        _hashSecret = GetRequiredSetting(configuration, "VNPAY_HASH_SECRET");
        _paymentUrl = GetRequiredSetting(configuration, "VNPAY_PAYMENT_URL");
        _returnUrl = GetRequiredSetting(configuration, "VNPAY_RETURN_URL");
        _ipnUrl = GetRequiredSetting(configuration, "VNPAY_IPN_URL");
        _version = configuration["VNPAY_VERSION"] ?? "2.1.0";
        _command = configuration["VNPAY_COMMAND"] ?? "pay";
        _currency = configuration["VNPAY_CURR_CODE"] ?? "VND";
        _locale = configuration["VNPAY_LOCALE"] ?? "vn";
        _orderType = configuration["VNPAY_ORDER_TYPE"] ?? "other";
        _frontendResultUrl = configuration["VNPAY_FRONTEND_RESULT_URL"] ?? "http://localhost:3000/thanh-toan/ket-qua";
        _vietnamTimeZone = ResolveVietnamTimeZone();
    }

    public async Task<CreateCourseOrderPaymentResultDto> CreateCourseOrderAsync(
        long userId,
        long courseId,
        string? clientIp,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new PaymentOperationException(404, "Khong tim thay nguoi dung.");
        }

        var course = await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == courseId && x.IsPublished, cancellationToken);
        if (course is null)
        {
            throw new PaymentOperationException(404, "Khong tim thay khoa hoc.");
        }

        if (course.IsFree || course.Price <= 0)
        {
            throw new PaymentOperationException(409, "Khoa hoc nay dang mien phi, khong can thanh toan.");
        }

        var userCourseExists = await _dbContext.UserCourses
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.CourseId == courseId, cancellationToken);
        if (userCourseExists)
        {
            throw new PaymentOperationException(409, "Ban da so huu khoa hoc nay.");
        }

        var nowUtc = DateTime.UtcNow;
        var normalizedClientIp = NormalizeClientIp(clientIp);

        var existingPendingOrder = await _dbContext.CourseOrders
            .AsTracking()
            .Where(x => x.UserId == userId && x.CourseId == courseId && x.Status == "pending")
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingPendingOrder is not null)
        {
            if (existingPendingOrder.ExpiredAt.HasValue && existingPendingOrder.ExpiredAt.Value <= nowUtc)
            {
                existingPendingOrder.MarkExpired("Don hang het han truoc khi tao lai.");
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                var refreshedExpiresAtUtc = existingPendingOrder.ExpiredAt ?? nowUtc.AddMinutes(15);
                var refreshedPaymentUrl = BuildPaymentUrl(
                    BuildPaymentParameters(
                        existingPendingOrder.Amount,
                        existingPendingOrder.VnpTxnRef ?? BuildTransactionRef(TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _vietnamTimeZone), userId, courseId),
                        existingPendingOrder.VnpOrderInfo ?? SanitizeOrderInfo($"Thanh toan khoa hoc {course.Title} {existingPendingOrder.OrderCode}"),
                        normalizedClientIp,
                        nowUtc,
                        refreshedExpiresAtUtc));

                existingPendingOrder.UpdatePaymentUrl(refreshedPaymentUrl);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new CreateCourseOrderPaymentResultDto(
                    existingPendingOrder.Id,
                    existingPendingOrder.OrderCode,
                    existingPendingOrder.Status,
                    existingPendingOrder.Amount,
                    existingPendingOrder.Currency,
                    refreshedPaymentUrl,
                    existingPendingOrder.ExpiredAt);
            }
        }

        var createdAtLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, _vietnamTimeZone);
        var expiredAtUtc = nowUtc.AddMinutes(15);
        var orderCode = BuildOrderCode(createdAtLocal, userId, courseId);
        var txnRef = BuildTransactionRef(createdAtLocal, userId, courseId);
        var orderInfo = SanitizeOrderInfo($"Thanh toan khoa hoc {course.Title} {orderCode}");

        var paymentParams = BuildPaymentParameters(
            course.Price,
            txnRef,
            orderInfo,
            normalizedClientIp,
            nowUtc,
            expiredAtUtc);

        var paymentUrl = BuildPaymentUrl(paymentParams);
        var order = new CourseOrder(
            orderCode,
            userId,
            courseId,
            course.Price,
            course.Currency,
            ProviderName,
            $"Thanh toan khoa hoc {course.Title}",
            $"Thanh toan khoa hoc {course.Title} qua VNPAY Sandbox.",
            txnRef,
            orderInfo,
            paymentUrl,
            _returnUrl,
            _ipnUrl,
            expiredAtUtc,
            normalizedClientIp);

        await using var dbTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        _dbContext.CourseOrders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var transaction = new PaymentTransaction(
            order.Id,
            ProviderName,
            "pay",
            "initiated",
            orderCode,
            txnRef,
            course.Price,
            course.Currency,
            null,
            null,
            JsonSerializer.Serialize(paymentParams, JsonOptions),
            null);

        _dbContext.PaymentTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await dbTransaction.CommitAsync(cancellationToken);

        return new CreateCourseOrderPaymentResultDto(
            order.Id,
            order.OrderCode,
            order.Status,
            order.Amount,
            order.Currency,
            paymentUrl,
            expiredAtUtc);
    }

    public async Task<CourseOrderStatusDto?> GetCourseOrderAsync(long orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CourseOrders
            .AsNoTracking()
            .Include(x => x.Course)
            .Where(x => x.Id == orderId)
            .Select(x => new CourseOrderStatusDto(
                x.Id,
                x.OrderCode,
                x.UserId,
                x.CourseId,
                x.Course != null ? x.Course.Title : string.Empty,
                x.Course != null ? x.Course.Slug : null,
                x.Status,
                x.Amount,
                x.Currency,
                x.CreatedAt,
                x.PaidAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseOrderStatusDto>> GetUserCourseOrdersAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CourseOrders
            .AsNoTracking()
            .Include(x => x.Course)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CourseOrderStatusDto(
                x.Id,
                x.OrderCode,
                x.UserId,
                x.CourseId,
                x.Course != null ? x.Course.Title : string.Empty,
                x.Course != null ? x.Course.Slug : null,
                x.Status,
                x.Amount,
                x.Currency,
                x.CreatedAt,
                x.PaidAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<VnPayReturnResultDto> HandleReturnAsync(
        IReadOnlyDictionary<string, string> query,
        CancellationToken cancellationToken = default)
    {
        var rawPayload = JsonSerializer.Serialize(query, JsonOptions);
        var isValidSignature = ValidateSignature(query);
        var responseCode = GetValue(query, "vnp_ResponseCode");
        var transactionStatus = GetValue(query, "vnp_TransactionStatus");
        var txnRef = GetValue(query, "vnp_TxnRef");

        var order = string.IsNullOrWhiteSpace(txnRef)
            ? null
            : await _dbContext.CourseOrders
                .Include(x => x.Course)
                .FirstOrDefaultAsync(x => x.VnpTxnRef == txnRef, cancellationToken);
        var transaction = order is null
            ? null
            : await _dbContext.PaymentTransactions
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);

        if (transaction is not null)
        {
            transaction.RecordReturn(
                responseCode,
                transactionStatus,
                GetValue(query, "vnp_TransactionNo"),
                GetValue(query, "vnp_BankCode"),
                GetValue(query, "vnp_BankTranNo"),
                GetValue(query, "vnp_CardType"),
                GetValue(query, "vnp_PayDate"),
                GetValue(query, "vnp_SecureHash"),
                rawPayload,
                rawPayload);
        }

        string resultCode;

        if (!isValidSignature)
        {
            if (transaction is not null)
            {
                transaction.UpdateStatus("invalid", "Callback return co checksum khong hop le.");
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            resultCode = "invalid_checksum";
        }
        else if (order is null || transaction is null)
        {
            resultCode = "failed";
        }
        else
        {
            resultCode = await ProcessOrderCallbackAsync(
                order,
                transaction,
                responseCode,
                transactionStatus,
                query,
                rawPayload,
                "return",
                cancellationToken);
        }

        var resultMessage = MapResultMessage(resultCode);
        var redirectUrl = BuildFrontendRedirectUrl(order?.Id, order?.OrderCode, resultCode, resultMessage);
        return new VnPayReturnResultDto(order?.Id, order?.OrderCode, resultCode, resultMessage, redirectUrl);
    }

    public async Task<VnPayIpnResponseDto> HandleIpnAsync(
        IReadOnlyDictionary<string, string> query,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateSignature(query))
        {
            _logger.LogWarning("VNPAY IPN invalid checksum: {Payload}", JsonSerializer.Serialize(query, JsonOptions));
            return new VnPayIpnResponseDto("97", "Invalid checksum");
        }

        var txnRef = GetValue(query, "vnp_TxnRef");
        if (string.IsNullOrWhiteSpace(txnRef))
        {
            return new VnPayIpnResponseDto("01", "Order not found");
        }

        var order = await _dbContext.CourseOrders
            .Include(x => x.Course)
            .FirstOrDefaultAsync(x => x.VnpTxnRef == txnRef, cancellationToken);
        if (order is null)
        {
            return new VnPayIpnResponseDto("01", "Order not found");
        }

        var transaction = await _dbContext.PaymentTransactions
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.OrderId == order.Id, cancellationToken);
        if (transaction is null)
        {
            return new VnPayIpnResponseDto("01", "Order not found");
        }

        var resultCode = await ProcessOrderCallbackAsync(
            order,
            transaction,
            GetValue(query, "vnp_ResponseCode"),
            GetValue(query, "vnp_TransactionStatus"),
            query,
            JsonSerializer.Serialize(query, JsonOptions),
            "ipn",
            cancellationToken);

        return resultCode switch
        {
            "invalid_amount" => new VnPayIpnResponseDto("04", "Invalid amount"),
            _ => new VnPayIpnResponseDto("00", order.Status == "paid" ? "Confirm Success" : "Order updated"),
        };
    }

    private async Task<string> ProcessOrderCallbackAsync(
        CourseOrder order,
        PaymentTransaction transaction,
        string? responseCode,
        string? transactionStatus,
        IReadOnlyDictionary<string, string> query,
        string rawPayload,
        string callbackSource,
        CancellationToken cancellationToken)
    {
        var queryAmount = ParseVnPayAmount(GetValue(query, "vnp_Amount"));
        if (queryAmount != order.Amount)
        {
            ApplyTransactionCallbackData(
                transaction,
                "invalid",
                responseCode,
                transactionStatus,
                query,
                rawPayload,
                callbackSource,
                "So tien callback khong khop voi don hang.");
            order.MarkFailed("So tien callback khong khop voi don hang.");
            await _dbContext.SaveChangesAsync(cancellationToken);
            return "invalid_amount";
        }

        if (order.Status == "paid")
        {
            ApplyTransactionCallbackData(
                transaction,
                "success",
                responseCode,
                transactionStatus,
                query,
                rawPayload,
                callbackSource,
                $"{callbackSource.ToUpperInvariant()} goi lap lai cho don da thanh toan.");
            await _dbContext.SaveChangesAsync(cancellationToken);
            return "success";
        }

        var isSuccess = responseCode == "00" && transactionStatus == "00";
        var successNote = callbackSource == "return"
            ? "Thanh toan thanh cong qua VNPAY Return."
            : "Thanh toan thanh cong qua VNPAY IPN.";
        var failedNote = callbackSource == "return"
            ? "Thanh toan khong thanh cong theo du lieu VNPAY Return."
            : "Thanh toan khong thanh cong theo du lieu VNPAY IPN.";

        ApplyTransactionCallbackData(
            transaction,
            isSuccess ? "success" : "failed",
            responseCode,
            transactionStatus,
            query,
            rawPayload,
            callbackSource,
            isSuccess ? successNote : failedNote);

        if (isSuccess)
        {
            order.MarkPaid(successNote);

            var userCourseExists = await _dbContext.UserCourses
                .AnyAsync(x => x.UserId == order.UserId && x.CourseId == order.CourseId, cancellationToken);
            if (!userCourseExists)
            {
                _dbContext.UserCourses.Add(new UserCourse(order.UserId, order.CourseId));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Auto trigger Payment Success Notification
            try
            {
                await _notificationService.SendNotificationAsync(
                    order.UserId,
                    "Xác nhận thanh toán thành công",
                    $"Đơn hàng {order.OrderCode} cho khóa học {order.Course?.Title ?? "đã chọn"} đã được xác nhận thanh toán thành công qua VNPAY. Bạn có thể bắt đầu học ngay!",
                    "payment",
                    "/profile",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi gửi thông báo thanh toán cho User {UserId}", order.UserId);
            }

            return "success";
        }

        if (responseCode == "24")
        {
            order.MarkCancelled("Nguoi dung huy giao dich tren VNPAY.");
            await _dbContext.SaveChangesAsync(cancellationToken);
            return "cancelled";
        }

        order.MarkFailed(failedNote);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return "failed";
    }

    private void ApplyTransactionCallbackData(
        PaymentTransaction transaction,
        string status,
        string? responseCode,
        string? transactionStatus,
        IReadOnlyDictionary<string, string> query,
        string rawPayload,
        string callbackSource,
        string note)
    {
        if (callbackSource == "ipn")
        {
            transaction.RecordIpn(
                status,
                responseCode,
                transactionStatus,
                GetValue(query, "vnp_TransactionNo"),
                GetValue(query, "vnp_BankCode"),
                GetValue(query, "vnp_BankTranNo"),
                GetValue(query, "vnp_CardType"),
                GetValue(query, "vnp_PayDate"),
                GetValue(query, "vnp_SecureHash"),
                rawPayload,
                rawPayload,
                note);
            return;
        }

        transaction.RecordReturn(
            responseCode,
            transactionStatus,
            GetValue(query, "vnp_TransactionNo"),
            GetValue(query, "vnp_BankCode"),
            GetValue(query, "vnp_BankTranNo"),
            GetValue(query, "vnp_CardType"),
            GetValue(query, "vnp_PayDate"),
            GetValue(query, "vnp_SecureHash"),
            rawPayload,
            rawPayload);
        transaction.UpdateStatus(status, note);
    }

    private string BuildPaymentUrl(IReadOnlyDictionary<string, string> parameters)
    {
        var filteredParameters = parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToList();
        var hashData = string.Join("&", filteredParameters.Select(x => $"{x.Key}={VnPayEncode(x.Value)}"));
        var secureHash = ComputeHmacSha512(_hashSecret, hashData);
        var queryString = string.Join("&", filteredParameters.Select(x => $"{x.Key}={VnPayEncode(x.Value)}"));

        return $"{_paymentUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    private SortedDictionary<string, string> BuildPaymentParameters(
        decimal amount,
        string txnRef,
        string orderInfo,
        string clientIp,
        DateTime createdAtUtc,
        DateTime expiredAtUtc)
    {
        var createdAtLocal = TimeZoneInfo.ConvertTimeFromUtc(createdAtUtc, _vietnamTimeZone);
        var expiredAtLocal = TimeZoneInfo.ConvertTimeFromUtc(expiredAtUtc, _vietnamTimeZone);

        return new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["vnp_Version"] = _version,
            ["vnp_Command"] = _command,
            ["vnp_TmnCode"] = _tmnCode,
            ["vnp_Amount"] = ConvertAmountToVnPay(amount),
            ["vnp_CreateDate"] = createdAtLocal.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
            ["vnp_CurrCode"] = _currency,
            ["vnp_IpAddr"] = clientIp,
            ["vnp_Locale"] = _locale,
            ["vnp_OrderInfo"] = orderInfo,
            ["vnp_OrderType"] = _orderType,
            ["vnp_ReturnUrl"] = _returnUrl,
            ["vnp_TxnRef"] = txnRef,
            ["vnp_ExpireDate"] = expiredAtLocal.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture),
        };
    }

    private bool ValidateSignature(IReadOnlyDictionary<string, string> query)
    {
        var providedHash = GetValue(query, "vnp_SecureHash");
        if (string.IsNullOrWhiteSpace(providedHash))
        {
            return false;
        }

        var filtered = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in query)
        {
            if (pair.Key is "vnp_SecureHash" or "vnp_SecureHashType")
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(pair.Value))
            {
                filtered[pair.Key] = pair.Value;
            }
        }

        var hashData = string.Join("&", filtered.Select(x => $"{x.Key}={VnPayEncode(x.Value)}"));
        var computedHash = ComputeHmacSha512(_hashSecret, hashData);
        return string.Equals(providedHash, computedHash, StringComparison.OrdinalIgnoreCase);
    }

    private string BuildFrontendRedirectUrl(long? orderId, string? orderCode, string resultCode, string resultMessage)
    {
        var uriBuilder = new UriBuilder(_frontendResultUrl);
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(uriBuilder.Query))
        {
            queryParts.Add(uriBuilder.Query.TrimStart('?'));
        }

        queryParts.Add($"resultCode={Uri.EscapeDataString(resultCode)}");
        queryParts.Add($"message={Uri.EscapeDataString(resultMessage)}");

        if (orderId.HasValue)
        {
            queryParts.Add($"orderId={orderId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(orderCode))
        {
            queryParts.Add($"orderCode={Uri.EscapeDataString(orderCode)}");
        }

        uriBuilder.Query = string.Join("&", queryParts.Where(x => !string.IsNullOrWhiteSpace(x)));
        return uriBuilder.ToString();
    }

    private static string BuildOrderCode(DateTime createdAtLocal, long userId, long courseId)
    {
        return $"CO-{createdAtLocal:yyyyMMddHHmmss}-{userId % 1000:D3}{courseId % 1000:D3}";
    }

    private static string BuildTransactionRef(DateTime createdAtLocal, long userId, long courseId)
    {
        return $"TXN{createdAtLocal:yyyyMMddHHmmss}{userId % 1000:D3}{courseId % 1000:D3}";
    }

    private static string ConvertAmountToVnPay(decimal amount)
    {
        var normalizedAmount = decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
        var vnpAmount = decimal.ToInt64(normalizedAmount * 100m);
        return vnpAmount.ToString(CultureInfo.InvariantCulture);
    }

    private static decimal ParseVnPayAmount(string? rawAmount)
    {
        if (!long.TryParse(rawAmount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var amount))
        {
            return -1;
        }

        return amount / 100m;
    }

    private static string ComputeHmacSha512(string key, string input)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
    }

    private static string VnPayEncode(string input)
    {
        return WebUtility.UrlEncode(input);
    }

    private static string SanitizeOrderInfo(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (character <= 127 && (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || character is '-' or '_' or '.'))
            {
                builder.Append(character);
            }
            else if (char.IsWhiteSpace(character))
            {
                builder.Append(' ');
            }
        }

        var sanitized = string.Join(" ", builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(sanitized) ? "Thanh toan khoa hoc" : sanitized;
    }

    private static string? GetValue(IReadOnlyDictionary<string, string> query, string key)
    {
        return query.TryGetValue(key, out var value) ? value : null;
    }

    private static string GetRequiredSetting(IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new InvalidOperationException($"Thieu cau hinh bat buoc: {key}");
    }

    private static string NormalizeClientIp(string? clientIp)
    {
        if (string.IsNullOrWhiteSpace(clientIp))
        {
            return "127.0.0.1";
        }

        return IPAddress.TryParse(clientIp, out _) ? clientIp : "127.0.0.1";
    }

    private static string MapResultMessage(string resultCode)
    {
        return resultCode switch
        {
            "success" => "Thanh toan thanh cong. Quyen hoc da duoc xac nhan.",
            "cancelled" => "Ban da huy giao dich thanh toan.",
            "invalid_checksum" => "Du lieu tra ve khong hop le.",
            "invalid_amount" => "So tien giao dich khong khop voi don hang.",
            _ => "Thanh toan chua thanh cong. Vui long thu lai.",
        };
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        foreach (var timeZoneId in new[] { "SE Asia Standard Time", "Asia/Bangkok" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
