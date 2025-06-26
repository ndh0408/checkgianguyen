/*
using Microsoft.AspNetCore.Mvc;
using GiaNguyenCheck.DTOs;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _paymentService.GetPaymentsAsync(page, pageSize);
            return Ok(result);
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var result = await _paymentService.CreatePaymentAsync(dto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var result = await _paymentService.GetPaymentAsync(id);
            return Ok(result);
        }

        // TODO: Implement these payment features later
        /*
        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
        {
            // TODO: Implement ProcessPaymentAsync
            return Ok(ApiResponse<bool>.SuccessResult(true, "Payment processing - TODO"));
        }

        [HttpPost("webhook/momo")]
        public async Task<IActionResult> MomoWebhook([FromBody] object webhookData)
        {
            // TODO: Implement ProcessMomoWebhookAsync
            return Ok(ApiResponse<bool>.SuccessResult(true, "Momo webhook - TODO"));
        }

        [HttpPost("webhook/stripe")]
        public async Task<IActionResult> StripeWebhook([FromBody] object webhookData)
        {
            var result = await _paymentService.ProcessStripeWebhookAsync(webhookData);
            return Ok(result);
        }

        [HttpPost("webhook/paypal")]
        public async Task<IActionResult> PayPalWebhook([FromBody] object webhookData)
        {
            var result = await _paymentService.ProcessPayPalWebhookAsync(webhookData);
            return Ok(result);
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetPlans()
        {
            var result = await _paymentService.GetPlansAsync();
            return Ok(result);
        }

        [HttpPost("upgrade-plan")]
        public async Task<IActionResult> UpgradePlan([FromBody] UpgradePlanDto dto)
        {
            var result = await _paymentService.UpgradePlanAsync(dto);
            return Ok(result);
        }

        [HttpGet("subscription/{tenantId}")]
        public async Task<IActionResult> GetSubscription(int tenantId)
        {
            var result = await _paymentService.GetSubscriptionAsync(tenantId);
            return Ok(result);
        }

        [HttpPost("cancel-subscription")]
        public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionDto dto)
        {
            var result = await _paymentService.CancelSubscriptionAsync(dto);
            return Ok(result);
        }

        [HttpGet("invoice/{paymentId}")]
        public async Task<IActionResult> GetInvoice(int paymentId)
        {
            var result = await _paymentService.GetInvoiceAsync(paymentId);
            return File(result.Data, "application/pdf", $"invoice-{paymentId}.pdf");
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetPaymentStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var result = await _paymentService.GetPaymentStatsAsync(fromDate, toDate);
            return Ok(result);
        }

        [HttpPost("refund")]
        public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentDto dto)
        {
            var result = await _paymentService.RefundPaymentAsync(dto);
            return Ok(result);
        }
        *//*
    }
}
*/ 