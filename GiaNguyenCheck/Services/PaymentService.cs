using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GiaNguyenCheck.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, int tenantId);
        Task<PaymentResponse> ProcessPaymentCallbackAsync(string paymentMethod, string callbackData);
        Task<PaymentResponse> GetPaymentStatusAsync(string paymentId, int tenantId);
        Task<List<PaymentResponse>> GetPaymentsByEventAsync(int eventId, int tenantId);
        Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount, int tenantId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IBaseRepository<EventPayment> _paymentRepository;
        private readonly IBaseRepository<Event> _eventRepository;
        private readonly IBaseRepository<Guest> _guestRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        
        public PaymentService(
            IBaseRepository<EventPayment> paymentRepository,
            IBaseRepository<Event> eventRepository,
            IBaseRepository<Guest> guestRepository,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _eventRepository = eventRepository;
            _guestRepository = guestRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request, int tenantId)
        {
            try
            {
                // Kiểm tra event tồn tại
                var eventEntity = await _eventRepository.GetByIdAsync(request.EventId);
                if (eventEntity == null || eventEntity.TenantId != tenantId)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Event không tồn tại"
                    };
                }

                // Kiểm tra guest tồn tại
                var guest = await _guestRepository.GetByIdAsync(request.GuestId);
                if (guest == null || guest.TenantId != tenantId)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Guest không tồn tại"
                    };
                }

                // Tạo payment record
                var payment = new EventPayment
                {
                    EventId = request.EventId,
                    GuestId = request.GuestId,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "VND",
                    PaymentMethod = request.PaymentMethod,
                    Status = EventPaymentStatus.Pending,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow
                };

                await _paymentRepository.AddAsync(payment);

                // Tạo payment URL dựa trên method
                var paymentUrl = await CreatePaymentUrlAsync(payment, request.PaymentMethod);

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    PaymentUrl = paymentUrl,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo payment");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi hệ thống"
                };
            }
        }

        public async Task<PaymentResponse> ProcessPaymentCallbackAsync(string paymentMethod, string callbackData)
        {
            try
            {
                var callback = JsonSerializer.Deserialize<PaymentCallback>(callbackData);
                if (callback == null)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Dữ liệu callback không hợp lệ"
                    };
                }

                var payment = await _paymentRepository.GetByIdAsync(int.Parse(callback.PaymentId));
                if (payment == null)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Payment không tồn tại"
                    };
                }

                // Xử lý callback dựa trên payment method
                switch (paymentMethod.ToLower())
                {
                    case "momo":
                        return await ProcessMomoCallbackAsync(payment, callback);
                    case "vnpay":
                        return await ProcessVNPayCallbackAsync(payment, callback);
                    case "stripe":
                        return await ProcessStripeCallbackAsync(payment, callback);
                    case "paypal":
                        return await ProcessPayPalCallbackAsync(payment, callback);
                    default:
                        return new PaymentResponse
                        {
                            Success = false,
                            ErrorMessage = "Payment method không được hỗ trợ"
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý payment callback");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi hệ thống"
                };
            }
        }

        public async Task<PaymentResponse> GetPaymentStatusAsync(string paymentId, int tenantId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(int.Parse(paymentId));
                if (payment == null || payment.TenantId != tenantId)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Payment không tồn tại"
                    };
                }

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Status = payment.Status.ToString(),
                    TransactionId = payment.TransactionId,
                    PaidAt = payment.PaidAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy payment status");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi hệ thống"
                };
            }
        }

        public async Task<List<PaymentResponse>> GetPaymentsByEventAsync(int eventId, int tenantId)
        {
            try
            {
                var payments = await _paymentRepository.GetAllAsync();
                var eventPayments = payments
                    .Where(p => p.EventId == eventId && p.TenantId == tenantId)
                    .Select(p => new PaymentResponse
                    {
                        Success = true,
                        PaymentId = p.Id.ToString(),
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Status = p.Status.ToString(),
                        TransactionId = p.TransactionId,
                        PaidAt = p.PaidAt,
                        CreatedAt = p.CreatedAt
                    })
                    .ToList();

                return eventPayments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy payments theo event");
                return new List<PaymentResponse>();
            }
        }

        public async Task<PaymentResponse> RefundPaymentAsync(string paymentId, decimal amount, int tenantId)
        {
            try
            {
                var payment = await _paymentRepository.GetByIdAsync(int.Parse(paymentId));
                if (payment == null || payment.TenantId != tenantId)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Payment không tồn tại"
                    };
                }

                if (payment.Status != EventPaymentStatus.Completed)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Payment chưa hoàn thành"
                    };
                }

                // Thực hiện refund
                payment.Status = EventPaymentStatus.Refunded;
                payment.RefundedAt = DateTime.UtcNow;
                payment.RefundedAmount = amount;

                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    Status = payment.Status.ToString(),
                    RefundedAmount = amount,
                    RefundedAt = payment.RefundedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi refund payment");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Lỗi hệ thống"
                };
            }
        }

        private async Task<string> CreatePaymentUrlAsync(EventPayment payment, string paymentMethod)
        {
            // Tạo payment URL dựa trên method
            switch (paymentMethod.ToLower())
            {
                case "momo":
                    return await CreateMomoPaymentUrlAsync(payment);
                case "vnpay":
                    return await CreateVNPayPaymentUrlAsync(payment);
                case "stripe":
                    return await CreateStripePaymentUrlAsync(payment);
                case "paypal":
                    return await CreatePayPalPaymentUrlAsync(payment);
                default:
                    throw new NotSupportedException($"Payment method {paymentMethod} không được hỗ trợ");
            }
        }

        private async Task<string> CreateMomoPaymentUrlAsync(EventPayment payment)
        {
            // Implement MoMo payment URL creation
            var momoConfig = _configuration.GetSection("Payment:MoMo");
            var partnerCode = momoConfig["PartnerCode"];
            var accessKey = momoConfig["AccessKey"];
            var secretKey = momoConfig["SecretKey"];
            var endpoint = momoConfig["Endpoint"];

            // Tạo request data cho MoMo
            var requestData = new
            {
                partnerCode = partnerCode,
                accessKey = accessKey,
                requestId = payment.Id.ToString(),
                amount = (long)payment.Amount,
                orderId = payment.Id.ToString(),
                orderInfo = $"Thanh toan event {payment.EventId}",
                redirectUrl = $"{_configuration["AppUrl"]}/payment/callback/momo",
                ipnUrl = $"{_configuration["AppUrl"]}/payment/ipn/momo",
                requestType = "captureWallet",
                extraData = ""
            };

            // Tạo signature và gửi request
            // Implementation chi tiết sẽ được thêm sau
            return $"{endpoint}?partnerCode={partnerCode}&requestId={payment.Id}";
        }

        private async Task<string> CreateVNPayPaymentUrlAsync(EventPayment payment)
        {
            // Implement VNPay payment URL creation
            var vnpayConfig = _configuration.GetSection("Payment:VNPay");
            var tmnCode = vnpayConfig["TmnCode"];
            var hashSecret = vnpayConfig["HashSecret"];
            var url = vnpayConfig["Url"];

            // Tạo VNPay request
            var vnpayRequest = new
            {
                vnp_Version = "2.1.0",
                vnp_Command = "pay",
                vnp_TmnCode = tmnCode,
                vnp_Amount = (long)(payment.Amount * 100), // VNPay tính bằng xu
                vnp_CurrCode = "VND",
                vnp_BankCode = "",
                vnp_TxnRef = payment.Id.ToString(),
                vnp_OrderInfo = $"Thanh toan event {payment.EventId}",
                vnp_OrderType = "other",
                vnp_Locale = "vn",
                vnp_ReturnUrl = $"{_configuration["AppUrl"]}/payment/callback/vnpay",
                vnp_IpAddr = "127.0.0.1",
                vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            // Tạo signature và URL
            return $"{url}?{string.Join("&", vnpayRequest.GetType().GetProperties().Select(p => $"{p.Name}={p.GetValue(vnpayRequest)}"))}";
        }

        private async Task<string> CreateStripePaymentUrlAsync(EventPayment payment)
        {
            // Implement Stripe payment URL creation
            var stripeConfig = _configuration.GetSection("Payment:Stripe");
            var publishableKey = stripeConfig["PublishableKey"];
            var secretKey = stripeConfig["SecretKey"];

            // Tạo Stripe checkout session
            // Implementation chi tiết sẽ được thêm sau
            return $"https://checkout.stripe.com/pay/{payment.Id}";
        }

        private async Task<string> CreatePayPalPaymentUrlAsync(EventPayment payment)
        {
            // Implement PayPal payment URL creation
            var paypalConfig = _configuration.GetSection("Payment:PayPal");
            var clientId = paypalConfig["ClientId"];
            var clientSecret = paypalConfig["ClientSecret"];

            // Tạo PayPal payment
            // Implementation chi tiết sẽ được thêm sau
            return $"https://www.paypal.com/cgi-bin/webscr?cmd=_xclick&business={clientId}&item_name=Event{payment.EventId}&amount={payment.Amount}";
        }

        private async Task<PaymentResponse> ProcessMomoCallbackAsync(EventPayment payment, PaymentCallback callback)
        {
            // Xử lý MoMo callback
            if (callback.ResultCode == "0")
            {
                payment.Status = EventPaymentStatus.Completed;
                payment.TransactionId = callback.TransactionId;
                payment.PaidAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    Status = payment.Status.ToString(),
                    TransactionId = payment.TransactionId
                };
            }
            else
            {
                payment.Status = EventPaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = callback.Message
                };
            }
        }

        private async Task<PaymentResponse> ProcessVNPayCallbackAsync(EventPayment payment, PaymentCallback callback)
        {
            // Xử lý VNPay callback
            if (callback.ResultCode == "00")
            {
                payment.Status = EventPaymentStatus.Completed;
                payment.TransactionId = callback.TransactionId;
                payment.PaidAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    Status = payment.Status.ToString(),
                    TransactionId = payment.TransactionId
                };
            }
            else
            {
                payment.Status = EventPaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = callback.Message
                };
            }
        }

        private async Task<PaymentResponse> ProcessStripeCallbackAsync(EventPayment payment, PaymentCallback callback)
        {
            // Xử lý Stripe callback
            if (callback.Status == "succeeded")
            {
                payment.Status = EventPaymentStatus.Completed;
                payment.TransactionId = callback.TransactionId;
                payment.PaidAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    Status = payment.Status.ToString(),
                    TransactionId = payment.TransactionId
                };
            }
            else
            {
                payment.Status = EventPaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = callback.Message
                };
            }
        }

        private async Task<PaymentResponse> ProcessPayPalCallbackAsync(EventPayment payment, PaymentCallback callback)
        {
            // Xử lý PayPal callback
            if (callback.Status == "COMPLETED")
            {
                payment.Status = EventPaymentStatus.Completed;
                payment.TransactionId = callback.TransactionId;
                payment.PaidAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id.ToString(),
                    Status = payment.Status.ToString(),
                    TransactionId = payment.TransactionId
                };
            }
            else
            {
                payment.Status = EventPaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);

                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = callback.Message
                };
            }
        }
    }

    public class PaymentCallback
    {
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}