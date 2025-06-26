using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface IRevenueOptimizationService
    {
        Task<decimal> CalculateDynamicPrice(int eventId);
        Task<PricingFactors> GetPricingFactors(int eventId);
        Task<decimal> GetHistoricalDemand(string eventType);
        Task<decimal[]> GetCompetitorPrices(string eventCategory);
        Task<double> GetOccupancyRate(int eventId);
        Task<OptimizationResult> OptimizeEventCapacity(int eventId);
        Task<RevenueImpact> CalculateRevenueImpact(double overbookingRate, int eventId);
    }

    public class PricingFactors
    {
        public int DaysUntilEvent { get; set; }
        public double CurrentOccupancy { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public decimal HistoricalDemand { get; set; }
        public decimal[] CompetitorPricing { get; set; }
        public string EventType { get; set; }
        public string EventCategory { get; set; }
        public bool IsHoliday { get; set; }
        public decimal BasePrice { get; set; }
        public int MaxGuests { get; set; }
        public int CurrentRegistrations { get; set; }
    }

    public class OptimizationResult
    {
        public int RecommendedCapacity { get; set; }
        public double ExpectedAttendance { get; set; }
        public RevenueImpact RevenueImpact { get; set; }
        public double RecommendedOverbookingRate { get; set; }
        public double HistoricalNoShowRate { get; set; }
        public string WeatherForecast { get; set; }
        public string Recommendation { get; set; }
    }

    public class RevenueImpact
    {
        public decimal CurrentRevenue { get; set; }
        public decimal OptimizedRevenue { get; set; }
        public decimal RevenueIncrease { get; set; }
        public double PercentageIncrease { get; set; }
        public int AdditionalGuests { get; set; }
    }
} 