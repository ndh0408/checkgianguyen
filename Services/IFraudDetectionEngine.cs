using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface IFraudDetectionEngine
    {
        Task<RiskScore> AnalyzeTransaction(CheckInAttempt attempt);
        Task<RiskScore> AnalyzePayment(PaymentAttempt attempt);
        Task<bool> IsSuspiciousActivity(int guestId, string activityType);
        Task FlagSuspiciousActivity(SuspiciousActivity activity);
        Task<FraudReport> GenerateFraudReport(DateTime fromDate, DateTime toDate);
        Task UpdateRiskProfile(int guestId, RiskScore riskScore);
        Task<List<FraudRule>> GetActiveFraudRules();
    }

    public class CheckInAttempt
    {
        public int GuestId { get; set; }
        public int EventId { get; set; }
        public string DeviceId { get; set; }
        public string IpAddress { get; set; }
        public string Location { get; set; }
        public DateTime Timestamp { get; set; }
        public CheckInType Type { get; set; }
        public string UserAgent { get; set; }
        public string QRCode { get; set; }
    }

    public class PaymentAttempt
    {
        public int GuestId { get; set; }
        public int EventId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string IpAddress { get; set; }
        public string DeviceId { get; set; }
        public DateTime Timestamp { get; set; }
        public string CardLast4 { get; set; }
        public string BillingAddress { get; set; }
    }

    public class RiskScore
    {
        public double Score { get; set; } // 0-100
        public RiskLevel Level { get; set; }
        public List<RiskFactor> Factors { get; set; }
        public string Recommendation { get; set; }
        public bool RequiresManualReview { get; set; }
        public bool ShouldBlock { get; set; }
    }

    public enum RiskLevel
    {
        Low = 0,      // 0-30
        Medium = 1,   // 31-60
        High = 2,     // 61-80
        Critical = 3  // 81-100
    }

    public class RiskFactor
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public string Description { get; set; }
        public bool IsTriggered { get; set; }
    }

    public class SuspiciousActivity
    {
        public int GuestId { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public RiskScore RiskScore { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class FraudReport
    {
        public DateTime GeneratedAt { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalTransactions { get; set; }
        public int SuspiciousTransactions { get; set; }
        public int BlockedTransactions { get; set; }
        public double FraudRate { get; set; }
        public List<SuspiciousActivity> TopSuspiciousActivities { get; set; }
        public Dictionary<string, int> RiskLevelDistribution { get; set; }
    }

    public class FraudRule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Condition { get; set; }
        public double RiskWeight { get; set; }
        public bool IsActive { get; set; }
        public FraudRuleType Type { get; set; }
    }

    public enum FraudRuleType
    {
        Velocity,
        Geographic,
        Behavioral,
        Device,
        Payment,
        Custom
    }
} 