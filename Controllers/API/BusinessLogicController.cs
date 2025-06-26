using GiaNguyenCheck.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BusinessLogicController : ControllerBase
    {
        private readonly IRevenueOptimizationService _revenueOptimizationService;
        private readonly INotificationOrchestrator _notificationOrchestrator;
        private readonly IFraudDetectionEngine _fraudDetectionEngine;
        private readonly ICapacityOptimizer _capacityOptimizer;
        private readonly IMetricsService _metricsService;

        public BusinessLogicController(
            IRevenueOptimizationService revenueOptimizationService,
            INotificationOrchestrator notificationOrchestrator,
            IFraudDetectionEngine fraudDetectionEngine,
            ICapacityOptimizer capacityOptimizer,
            IMetricsService metricsService)
        {
            _revenueOptimizationService = revenueOptimizationService;
            _notificationOrchestrator = notificationOrchestrator;
            _fraudDetectionEngine = fraudDetectionEngine;
            _capacityOptimizer = capacityOptimizer;
            _metricsService = metricsService;
        }

        [HttpGet("revenue-optimization/{eventId}")]
        public async Task<IActionResult> GetRevenueOptimization(int eventId)
        {
            try
            {
                var dynamicPrice = await _revenueOptimizationService.CalculateDynamicPrice(eventId);
                var pricingFactors = await _revenueOptimizationService.GetPricingFactors(eventId);
                var capacityOptimization = await _revenueOptimizationService.OptimizeEventCapacity(eventId);

                var result = new
                {
                    EventId = eventId,
                    DynamicPrice = dynamicPrice,
                    PricingFactors = pricingFactors,
                    CapacityOptimization = capacityOptimization,
                    GeneratedAt = DateTime.UtcNow
                };

                _metricsService.RecordApiCall("GetRevenueOptimization", 0.1, true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("notifications/smart-reminders/{eventId}")]
        public async Task<IActionResult> SendSmartReminders(int eventId)
        {
            try
            {
                await _notificationOrchestrator.SendSmartReminders(eventId);
                
                _metricsService.RecordApiCall("SendSmartReminders", 0.1, true);
                return Ok(new { message = "Smart reminders scheduled successfully", eventId });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("notifications/schedule-optimal/{eventId}")]
        public async Task<IActionResult> ScheduleOptimalNotifications(int eventId)
        {
            try
            {
                await _notificationOrchestrator.ScheduleOptimalNotifications(eventId);
                
                _metricsService.RecordApiCall("ScheduleOptimalNotifications", 0.1, true);
                return Ok(new { message = "Optimal notifications scheduled successfully", eventId });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("fraud-detection/analyze-checkin")]
        public async Task<IActionResult> AnalyzeCheckInTransaction([FromBody] CheckInAttempt attempt)
        {
            try
            {
                var riskScore = await _fraudDetectionEngine.AnalyzeTransaction(attempt);
                
                _metricsService.RecordApiCall("AnalyzeCheckInTransaction", 0.2, true);
                return Ok(new { riskScore, attempt });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("fraud-detection/analyze-payment")]
        public async Task<IActionResult> AnalyzePaymentTransaction([FromBody] PaymentAttempt attempt)
        {
            try
            {
                var riskScore = await _fraudDetectionEngine.AnalyzePayment(attempt);
                
                _metricsService.RecordApiCall("AnalyzePaymentTransaction", 0.2, true);
                return Ok(new { riskScore, attempt });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("fraud-detection/report")]
        public async Task<IActionResult> GetFraudReport([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
        {
            try
            {
                var report = await _fraudDetectionEngine.GenerateFraudReport(fromDate, toDate);
                
                _metricsService.RecordApiCall("GetFraudReport", 0.1, true);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("capacity-optimization/{eventId}")]
        public async Task<IActionResult> GetCapacityOptimization(int eventId)
        {
            try
            {
                var optimization = await _capacityOptimizer.OptimizeEventCapacity(eventId);
                var recommendation = await _capacityOptimizer.GetCapacityRecommendation(eventId);
                var factors = await _capacityOptimizer.GetCapacityFactors(eventId);

                var result = new
                {
                    EventId = eventId,
                    Optimization = optimization,
                    Recommendation = recommendation,
                    Factors = factors,
                    GeneratedAt = DateTime.UtcNow
                };

                _metricsService.RecordApiCall("GetCapacityOptimization", 0.1, true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("capacity-optimization/{eventId}/revenue-impact")]
        public async Task<IActionResult> GetRevenueImpact(int eventId, [FromQuery] double overbookingRate = 0.15)
        {
            try
            {
                var revenueImpact = await _capacityOptimizer.CalculateRevenueImpact(overbookingRate, eventId);
                
                _metricsService.RecordApiCall("GetRevenueImpact", 0.05, true);
                return Ok(new { eventId, overbookingRate, revenueImpact });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("notifications/optimal-time/{guestId}")]
        public async Task<IActionResult> GetOptimalSendTime(int guestId, [FromQuery] NotificationType type = NotificationType.EventReminder)
        {
            try
            {
                var optimalTime = await _notificationOrchestrator.CalculateOptimalSendTime(guestId, type);
                var preferredChannel = await _notificationOrchestrator.DeterminePreferredChannel(guestId);
                var preferences = await _notificationOrchestrator.GetGuestPreferences(guestId);

                var result = new
                {
                    GuestId = guestId,
                    NotificationType = type,
                    OptimalSendTime = optimalTime,
                    PreferredChannel = preferredChannel,
                    Preferences = preferences
                };

                _metricsService.RecordApiCall("GetOptimalSendTime", 0.05, true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("fraud-detection/suspicious-activity")]
        public async Task<IActionResult> FlagSuspiciousActivity([FromBody] SuspiciousActivity activity)
        {
            try
            {
                await _fraudDetectionEngine.FlagSuspiciousActivity(activity);
                
                _metricsService.RecordApiCall("FlagSuspiciousActivity", 0.1, true);
                return Ok(new { message = "Suspicious activity flagged successfully", activity });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("fraud-detection/rules")]
        public async Task<IActionResult> GetFraudRules()
        {
            try
            {
                var rules = await _fraudDetectionEngine.GetActiveFraudRules();
                
                _metricsService.RecordApiCall("GetFraudRules", 0.05, true);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("revenue-optimization/{eventId}/pricing-factors")]
        public async Task<IActionResult> GetPricingFactors(int eventId)
        {
            try
            {
                var factors = await _revenueOptimizationService.GetPricingFactors(eventId);
                
                _metricsService.RecordApiCall("GetPricingFactors", 0.05, true);
                return Ok(new { eventId, factors });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("capacity-optimization/{eventId}/overbooking-strategy")]
        public async Task<IActionResult> GetOverbookingStrategy(int eventId)
        {
            try
            {
                var strategy = await _capacityOptimizer.CalculateOverbookingStrategy(eventId);
                
                _metricsService.RecordApiCall("GetOverbookingStrategy", 0.05, true);
                return Ok(new { eventId, strategy });
            }
            catch (Exception ex)
            {
                _metricsService.RecordError("BusinessLogic", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }
    }
} 