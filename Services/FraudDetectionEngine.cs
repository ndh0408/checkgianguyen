using GiaNguyenCheck.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class FraudDetectionEngine : IFraudDetectionEngine
    {
        private readonly ICheckInRepository _checkInRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<FraudDetectionEngine> _logger;

        public FraudDetectionEngine(
            ICheckInRepository checkInRepository,
            IPaymentRepository paymentRepository,
            IGuestRepository guestRepository,
            ICacheService cacheService,
            IMetricsService metricsService,
            ILogger<FraudDetectionEngine> logger)
        {
            _checkInRepository = checkInRepository;
            _paymentRepository = paymentRepository;
            _guestRepository = guestRepository;
            _cacheService = cacheService;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task<RiskScore> AnalyzeTransaction(CheckInAttempt attempt)
        {
            try
            {
                _logger.LogInformation("Analyzing check-in transaction for guest {GuestId}", attempt.GuestId);
                
                var riskFactors = new List<RiskFactor>
                {
                    await CheckVelocity(attempt),
                    await CheckDeviceReputation(attempt),
                    await CheckGeolocation(attempt),
                    await CheckBehaviorPattern(attempt),
                    await CheckQRCodeValidity(attempt),
                    await CheckTimeAnomaly(attempt)
                };

                var score = CalculateRiskScore(riskFactors);
                var riskLevel = DetermineRiskLevel(score);
                var recommendation = GenerateRecommendation(score, riskFactors);
                var requiresReview = score > 70 || riskFactors.Any(f => f.Weight > 0.8);
                var shouldBlock = score > 90;

                var riskScore = new RiskScore
                {
                    Score = score,
                    Level = riskLevel,
                    Factors = riskFactors,
                    Recommendation = recommendation,
                    RequiresManualReview = requiresReview,
                    ShouldBlock = shouldBlock
                };

                if (shouldBlock)
                {
                    await FlagSuspiciousActivity(new SuspiciousActivity
                    {
                        GuestId = attempt.GuestId,
                        ActivityType = "CheckInBlocked",
                        Description = $"Check-in blocked due to high risk score: {score}",
                        RiskScore = riskScore,
                        Timestamp = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            { "EventId", attempt.EventId },
                            { "DeviceId", attempt.DeviceId },
                            { "IpAddress", attempt.IpAddress },
                            { "Location", attempt.Location }
                        }
                    });
                }

                _metricsService.RecordApiCall("AnalyzeTransaction", 0.2, true);
                _logger.LogInformation("Transaction analysis completed for guest {GuestId}: Score={Score}, Level={Level}", 
                    attempt.GuestId, score, riskLevel);

                return riskScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing transaction for guest {GuestId}", attempt.GuestId);
                _metricsService.RecordError("FraudDetection", ex.Message);
                throw;
            }
        }

        public async Task<RiskScore> AnalyzePayment(PaymentAttempt attempt)
        {
            try
            {
                _logger.LogInformation("Analyzing payment transaction for guest {GuestId}", attempt.GuestId);
                
                var riskFactors = new List<RiskFactor>
                {
                    await CheckPaymentVelocity(attempt),
                    await CheckCardReputation(attempt),
                    await CheckAmountAnomaly(attempt),
                    await CheckGeographicMismatch(attempt),
                    await CheckDeviceHistory(attempt),
                    await CheckGuestPaymentHistory(attempt)
                };

                var score = CalculateRiskScore(riskFactors);
                var riskLevel = DetermineRiskLevel(score);
                var recommendation = GenerateRecommendation(score, riskFactors);
                var requiresReview = score > 60 || riskFactors.Any(f => f.Weight > 0.7);
                var shouldBlock = score > 85;

                var riskScore = new RiskScore
                {
                    Score = score,
                    Level = riskLevel,
                    Factors = riskFactors,
                    Recommendation = recommendation,
                    RequiresManualReview = requiresReview,
                    ShouldBlock = shouldBlock
                };

                if (shouldBlock)
                {
                    await FlagSuspiciousActivity(new SuspiciousActivity
                    {
                        GuestId = attempt.GuestId,
                        ActivityType = "PaymentBlocked",
                        Description = $"Payment blocked due to high risk score: {score}",
                        RiskScore = riskScore,
                        Timestamp = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            { "EventId", attempt.EventId },
                            { "Amount", attempt.Amount },
                            { "Method", attempt.Method.ToString() },
                            { "CardLast4", attempt.CardLast4 }
                        }
                    });
                }

                _metricsService.RecordApiCall("AnalyzePayment", 0.2, true);
                _logger.LogInformation("Payment analysis completed for guest {GuestId}: Score={Score}, Level={Level}", 
                    attempt.GuestId, score, riskLevel);

                return riskScore;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing payment for guest {GuestId}", attempt.GuestId);
                _metricsService.RecordError("FraudDetection", ex.Message);
                throw;
            }
        }

        public async Task<bool> IsSuspiciousActivity(int guestId, string activityType)
        {
            var cacheKey = $"suspicious_activity_{guestId}_{activityType}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Check recent suspicious activities
                var recentActivities = await GetRecentSuspiciousActivities(guestId, TimeSpan.FromHours(24));
                return recentActivities.Any(a => a.ActivityType == activityType && a.RiskScore.Score > 70);
            }, TimeSpan.FromMinutes(5));
        }

        public async Task FlagSuspiciousActivity(SuspiciousActivity activity)
        {
            try
            {
                // Store suspicious activity
                await StoreSuspiciousActivity(activity);
                
                // Update guest risk profile
                await UpdateRiskProfile(activity.GuestId, activity.RiskScore);
                
                // Send alert to security team if critical
                if (activity.RiskScore.Level == RiskLevel.Critical)
                {
                    await SendSecurityAlert(activity);
                }

                _metricsService.RecordApiCall("FlagSuspiciousActivity", 0.1, true);
                _logger.LogWarning("Flagged suspicious activity: {ActivityType} for guest {GuestId} with score {Score}", 
                    activity.ActivityType, activity.GuestId, activity.RiskScore.Score);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flagging suspicious activity for guest {GuestId}", activity.GuestId);
                _metricsService.RecordError("FraudDetection", ex.Message);
            }
        }

        public async Task<FraudReport> GenerateFraudReport(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var cacheKey = $"fraud_report_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}";
                return await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    var totalTransactions = await GetTotalTransactions(fromDate, toDate);
                    var suspiciousTransactions = await GetSuspiciousTransactions(fromDate, toDate);
                    var blockedTransactions = await GetBlockedTransactions(fromDate, toDate);
                    var fraudRate = totalTransactions > 0 ? (double)suspiciousTransactions / totalTransactions : 0;

                    var report = new FraudReport
                    {
                        GeneratedAt = DateTime.UtcNow,
                        FromDate = fromDate,
                        ToDate = toDate,
                        TotalTransactions = totalTransactions,
                        SuspiciousTransactions = suspiciousTransactions,
                        BlockedTransactions = blockedTransactions,
                        FraudRate = fraudRate,
                        TopSuspiciousActivities = await GetTopSuspiciousActivities(fromDate, toDate, 10),
                        RiskLevelDistribution = await GetRiskLevelDistribution(fromDate, toDate)
                    };

                    _logger.LogInformation("Generated fraud report: {TotalTransactions} transactions, {FraudRate:P2} fraud rate", 
                        totalTransactions, fraudRate);

                    return report;
                }, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating fraud report from {FromDate} to {ToDate}", fromDate, toDate);
                throw;
            }
        }

        public async Task UpdateRiskProfile(int guestId, RiskScore riskScore)
        {
            try
            {
                var cacheKey = $"risk_profile_{guestId}";
                await _cacheService.SetAsync(cacheKey, riskScore, TimeSpan.FromHours(24));
                
                _logger.LogDebug("Updated risk profile for guest {GuestId}: Score={Score}, Level={Level}", 
                    guestId, riskScore.Score, riskScore.Level);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating risk profile for guest {GuestId}", guestId);
            }
        }

        public async Task<List<FraudRule>> GetActiveFraudRules()
        {
            var cacheKey = "active_fraud_rules";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate fraud rules (in production, this would come from database)
                return new List<FraudRule>
                {
                    new FraudRule
                    {
                        Id = 1,
                        Name = "High Velocity Check-in",
                        Description = "Multiple check-ins in short time period",
                        Condition = "check_ins > 5 in 5 minutes",
                        RiskWeight = 0.8,
                        IsActive = true,
                        Type = FraudRuleType.Velocity
                    },
                    new FraudRule
                    {
                        Id = 2,
                        Name = "Geographic Anomaly",
                        Description = "Check-in from unusual location",
                        Condition = "distance > 100km from usual location",
                        RiskWeight = 0.7,
                        IsActive = true,
                        Type = FraudRuleType.Geographic
                    },
                    new FraudRule
                    {
                        Id = 3,
                        Name = "Device Reputation",
                        Description = "Check-in from suspicious device",
                        Condition = "device_risk_score > 0.8",
                        RiskWeight = 0.9,
                        IsActive = true,
                        Type = FraudRuleType.Device
                    },
                    new FraudRule
                    {
                        Id = 4,
                        Name = "Payment Velocity",
                        Description = "Multiple payment attempts",
                        Condition = "payments > 3 in 1 hour",
                        RiskWeight = 0.8,
                        IsActive = true,
                        Type = FraudRuleType.Payment
                    }
                };
            }, TimeSpan.FromHours(6));
        }

        private async Task<RiskFactor> CheckVelocity(CheckInAttempt attempt)
        {
            var cacheKey = $"velocity_check_{attempt.GuestId}_{attempt.EventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var recentCheckIns = await GetRecentCheckIns(attempt.GuestId, TimeSpan.FromMinutes(5));
                var count = recentCheckIns.Count;
                
                var weight = count switch
                {
                    > 10 => 1.0,  // Critical
                    > 5 => 0.8,   // High
                    > 3 => 0.6,   // Medium
                    > 1 => 0.4,   // Low
                    _ => 0.0      // Normal
                };

                return new RiskFactor
                {
                    Name = "Velocity Check",
                    Weight = weight,
                    Description = $"{count} check-ins in last 5 minutes",
                    IsTriggered = weight > 0.3
                };
            }, TimeSpan.FromMinutes(1));
        }

        private async Task<RiskFactor> CheckDeviceReputation(CheckInAttempt attempt)
        {
            var cacheKey = $"device_reputation_{attempt.DeviceId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var deviceCheckIns = await GetCheckInsByDevice(attempt.DeviceId, TimeSpan.FromDays(7));
                var uniqueGuests = deviceCheckIns.Select(c => c.GuestId).Distinct().Count();
                
                var weight = uniqueGuests switch
                {
                    > 20 => 1.0,  // Critical - device used by many guests
                    > 10 => 0.8,  // High
                    > 5 => 0.6,   // Medium
                    > 2 => 0.4,   // Low
                    _ => 0.0      // Normal
                };

                return new RiskFactor
                {
                    Name = "Device Reputation",
                    Weight = weight,
                    Description = $"Device used by {uniqueGuests} different guests",
                    IsTriggered = weight > 0.3
                };
            }, TimeSpan.FromMinutes(5));
        }

        private async Task<RiskFactor> CheckGeolocation(CheckInAttempt attempt)
        {
            // Simulate geolocation check
            var weight = 0.0;
            var description = "Location appears normal";

            // Check if location is too far from event location
            if (!string.IsNullOrEmpty(attempt.Location))
            {
                // Simulate distance calculation
                var distance = new Random().Next(0, 200); // km
                weight = distance switch
                {
                    > 150 => 0.9,  // Critical - very far
                    > 100 => 0.7,  // High
                    > 50 => 0.5,   // Medium
                    > 20 => 0.3,   // Low
                    _ => 0.0       // Normal
                };

                description = $"Check-in from {distance}km away";
            }

            return new RiskFactor
            {
                Name = "Geolocation",
                Weight = weight,
                Description = description,
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckBehaviorPattern(CheckInAttempt attempt)
        {
            // Simulate behavior pattern analysis
            var guestHistory = await GetGuestCheckInHistory(attempt.GuestId);
            var usualTime = guestHistory.Any() ? guestHistory.Average(h => h.Timestamp.Hour) : 12;
            var currentHour = attempt.Timestamp.Hour;
            var timeDiff = Math.Abs(currentHour - usualTime);

            var weight = timeDiff switch
            {
                > 8 => 0.6,   // Unusual time
                > 4 => 0.4,   // Somewhat unusual
                _ => 0.0      // Normal time
            };

            return new RiskFactor
            {
                Name = "Behavior Pattern",
                Weight = weight,
                Description = $"Check-in at {currentHour}:00 (usual: {usualTime}:00)",
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckQRCodeValidity(CheckInAttempt attempt)
        {
            // Simulate QR code validation
            var isValid = !string.IsNullOrEmpty(attempt.QRCode) && attempt.QRCode.Length > 10;
            var weight = isValid ? 0.0 : 0.9;

            return new RiskFactor
            {
                Name = "QR Code Validity",
                Weight = weight,
                Description = isValid ? "Valid QR code" : "Invalid or missing QR code",
                IsTriggered = !isValid
            };
        }

        private async Task<RiskFactor> CheckTimeAnomaly(CheckInAttempt attempt)
        {
            // Check if check-in is outside event time window
            var eventEntity = await _eventRepository.GetByIdAsync(attempt.EventId);
            if (eventEntity == null)
                return new RiskFactor { Name = "Time Anomaly", Weight = 0.0, Description = "Event not found", IsTriggered = false };

            var isWithinWindow = attempt.Timestamp >= eventEntity.StartTime.AddHours(-1) && 
                                attempt.Timestamp <= eventEntity.EndTime.AddHours(1);

            var weight = isWithinWindow ? 0.0 : 0.7;

            return new RiskFactor
            {
                Name = "Time Anomaly",
                Weight = weight,
                Description = isWithinWindow ? "Within event time window" : "Outside event time window",
                IsTriggered = !isWithinWindow
            };
        }

        private async Task<RiskFactor> CheckPaymentVelocity(PaymentAttempt attempt)
        {
            var recentPayments = await GetRecentPayments(attempt.GuestId, TimeSpan.FromHours(1));
            var count = recentPayments.Count;
            
            var weight = count switch
            {
                > 5 => 1.0,   // Critical
                > 3 => 0.8,   // High
                > 2 => 0.6,   // Medium
                > 1 => 0.4,   // Low
                _ => 0.0      // Normal
            };

            return new RiskFactor
            {
                Name = "Payment Velocity",
                Weight = weight,
                Description = $"{count} payment attempts in last hour",
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckCardReputation(PaymentAttempt attempt)
        {
            // Simulate card reputation check
            var cardCheckIns = await GetCheckInsByCard(attempt.CardLast4, TimeSpan.FromDays(30));
            var uniqueGuests = cardCheckIns.Select(c => c.GuestId).Distinct().Count();
            
            var weight = uniqueGuests switch
            {
                > 10 => 0.9,  // Critical
                > 5 => 0.7,   // High
                > 2 => 0.5,   // Medium
                _ => 0.0      // Normal
            };

            return new RiskFactor
            {
                Name = "Card Reputation",
                Weight = weight,
                Description = $"Card used by {uniqueGuests} different guests",
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckAmountAnomaly(PaymentAttempt attempt)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(attempt.EventId);
            if (eventEntity == null)
                return new RiskFactor { Name = "Amount Anomaly", Weight = 0.0, Description = "Event not found", IsTriggered = false };

            var expectedAmount = 100000m; // Expected ticket price
            var amountRatio = attempt.Amount / expectedAmount;
            
            var weight = amountRatio switch
            {
                > 3 => 0.8,   // Very high amount
                > 2 => 0.6,   // High amount
                < 0.5 => 0.7, // Very low amount
                < 0.8 => 0.4, // Low amount
                _ => 0.0      // Normal amount
            };

            return new RiskFactor
            {
                Name = "Amount Anomaly",
                Weight = weight,
                Description = $"Amount {attempt.Amount:N0} VND (expected: {expectedAmount:N0} VND)",
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckGeographicMismatch(PaymentAttempt attempt)
        {
            // Simulate geographic mismatch check
            var isForeignCard = !string.IsNullOrEmpty(attempt.CardLast4) && attempt.CardLast4.StartsWith("4");
            var isForeignIP = !string.IsNullOrEmpty(attempt.IpAddress) && !attempt.IpAddress.StartsWith("192.168");
            
            var weight = (isForeignCard && isForeignIP) ? 0.6 : 0.0;

            return new RiskFactor
            {
                Name = "Geographic Mismatch",
                Weight = weight,
                Description = isForeignCard && isForeignIP ? "Foreign card and IP detected" : "No geographic mismatch",
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckDeviceHistory(PaymentAttempt attempt)
        {
            var devicePayments = await GetPaymentsByDevice(attempt.DeviceId, TimeSpan.FromDays(7));
            var uniqueGuests = devicePayments.Select(p => p.GuestId).Distinct().Count();
            
            var weight = uniqueGuests switch
            {
                > 15 => 0.9,  // Critical
                > 8 => 0.7,   // High
                > 3 => 0.5,   // Medium
                _ => 0.0      // Normal
            };

            return new RiskFactor
            {
                Name = "Device History",
                Weight = weight,
                Description = $"Device used by {uniqueGuests} different guests for payments",
                IsTriggered = weight > 0.3
            };
        }

        private async Task<RiskFactor> CheckGuestPaymentHistory(PaymentAttempt attempt)
        {
            var guestPayments = await GetGuestPaymentHistory(attempt.GuestId, TimeSpan.FromDays(90));
            var failedPayments = guestPayments.Count(p => p.Status == PaymentStatus.Failed);
            var totalPayments = guestPayments.Count;
            
            var failureRate = totalPayments > 0 ? (double)failedPayments / totalPayments : 0;
            var weight = failureRate switch
            {
                > 0.5 => 0.8,  // High failure rate
                > 0.3 => 0.6,  // Medium failure rate
                > 0.1 => 0.4,  // Low failure rate
                _ => 0.0       // Normal
            };

            return new RiskFactor
            {
                Name = "Payment History",
                Weight = weight,
                Description = $"{failureRate:P1} payment failure rate ({failedPayments}/{totalPayments})",
                IsTriggered = weight > 0.3
            };
        }

        private double CalculateRiskScore(List<RiskFactor> factors)
        {
            var totalWeight = factors.Sum(f => f.Weight);
            var triggeredFactors = factors.Where(f => f.IsTriggered).ToList();
            var triggeredWeight = triggeredFactors.Sum(f => f.Weight);
            
            // Normalize to 0-100 scale
            var score = (triggeredWeight / Math.Max(totalWeight, 1)) * 100;
            
            return Math.Min(score, 100); // Cap at 100
        }

        private RiskLevel DetermineRiskLevel(double score)
        {
            return score switch
            {
                >= 81 => RiskLevel.Critical,
                >= 61 => RiskLevel.High,
                >= 31 => RiskLevel.Medium,
                _ => RiskLevel.Low
            };
        }

        private string GenerateRecommendation(double score, List<RiskFactor> factors)
        {
            var triggeredFactors = factors.Where(f => f.IsTriggered).ToList();
            
            if (score >= 90)
                return "BLOCK: Critical risk detected. Manual review required.";
            else if (score >= 70)
                return "REVIEW: High risk detected. Additional verification recommended.";
            else if (score >= 50)
                return "MONITOR: Medium risk detected. Watch for suspicious patterns.";
            else if (score >= 30)
                return "CAUTION: Low risk detected. Proceed with normal processing.";
            else
                return "CLEAR: No significant risk factors detected.";
        }

        // Stub methods for data access
        private async Task<List<dynamic>> GetRecentCheckIns(int guestId, TimeSpan timeSpan) => new List<dynamic>();
        private async Task<List<dynamic>> GetCheckInsByDevice(string deviceId, TimeSpan timeSpan) => new List<dynamic>();
        private async Task<List<dynamic>> GetGuestCheckInHistory(int guestId) => new List<dynamic>();
        private async Task<List<dynamic>> GetRecentPayments(int guestId, TimeSpan timeSpan) => new List<dynamic>();
        private async Task<List<dynamic>> GetCheckInsByCard(string cardLast4, TimeSpan timeSpan) => new List<dynamic>();
        private async Task<List<dynamic>> GetPaymentsByDevice(string deviceId, TimeSpan timeSpan) => new List<dynamic>();
        private async Task<List<dynamic>> GetGuestPaymentHistory(int guestId, TimeSpan timeSpan) => new List<dynamic>();
        private async Task<List<SuspiciousActivity>> GetRecentSuspiciousActivities(int guestId, TimeSpan timeSpan) => new List<SuspiciousActivity>();
        private async Task StoreSuspiciousActivity(SuspiciousActivity activity) { }
        private async Task SendSecurityAlert(SuspiciousActivity activity) { }
        private async Task<int> GetTotalTransactions(DateTime fromDate, DateTime toDate) => 1000;
        private async Task<int> GetSuspiciousTransactions(DateTime fromDate, DateTime toDate) => 50;
        private async Task<int> GetBlockedTransactions(DateTime fromDate, DateTime toDate) => 10;
        private async Task<List<SuspiciousActivity>> GetTopSuspiciousActivities(DateTime fromDate, DateTime toDate, int count) => new List<SuspiciousActivity>();
        private async Task<Dictionary<string, int>> GetRiskLevelDistribution(DateTime fromDate, DateTime toDate) => new Dictionary<string, int>();
    }
} 