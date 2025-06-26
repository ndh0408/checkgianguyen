using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface ICapacityOptimizer
    {
        Task<OptimizationResult> OptimizeEventCapacity(int eventId);
        Task<double> GetHistoricalNoShowRate(int eventId);
        Task<string> GetWeatherImpact(int eventId);
        Task<CapacityRecommendation> GetCapacityRecommendation(int eventId);
        Task<OverbookingStrategy> CalculateOverbookingStrategy(int eventId);
        Task<RevenueImpact> CalculateRevenueImpact(double overbookingRate, int eventId);
        Task<List<CapacityFactor>> GetCapacityFactors(int eventId);
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
        public List<CapacityFactor> Factors { get; set; }
        public OverbookingStrategy Strategy { get; set; }
    }

    public class CapacityRecommendation
    {
        public int BaseCapacity { get; set; }
        public int OptimizedCapacity { get; set; }
        public double OverbookingRate { get; set; }
        public double ExpectedNoShowRate { get; set; }
        public double ConfidenceLevel { get; set; }
        public string Reasoning { get; set; }
        public List<string> Warnings { get; set; }
    }

    public class OverbookingStrategy
    {
        public double ConservativeRate { get; set; }
        public double ModerateRate { get; set; }
        public double AggressiveRate { get; set; }
        public double RecommendedRate { get; set; }
        public string StrategyType { get; set; }
        public Dictionary<string, double> RiskFactors { get; set; }
    }

    public class CapacityFactor
    {
        public string Name { get; set; }
        public double Weight { get; set; }
        public string Description { get; set; }
        public double Impact { get; set; }
        public bool IsPositive { get; set; }
    }

    public class RevenueImpact
    {
        public decimal CurrentRevenue { get; set; }
        public decimal OptimizedRevenue { get; set; }
        public decimal RevenueIncrease { get; set; }
        public double PercentageIncrease { get; set; }
        public int AdditionalGuests { get; set; }
        public decimal CostPerGuest { get; set; }
        public decimal NetProfitIncrease { get; set; }
    }
} 