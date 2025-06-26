using GiaNguyenCheck.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class RevenueOptimizationService : IRevenueOptimizationService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<RevenueOptimizationService> _logger;

        public RevenueOptimizationService(
            IEventRepository eventRepository,
            IGuestRepository guestRepository,
            IPaymentRepository paymentRepository,
            ICacheService cacheService,
            IMetricsService metricsService,
            ILogger<RevenueOptimizationService> logger)
        {
            _eventRepository = eventRepository;
            _guestRepository = guestRepository;
            _paymentRepository = paymentRepository;
            _cacheService = cacheService;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task<decimal> CalculateDynamicPrice(int eventId)
        {
            try
            {
                var factors = await GetPricingFactors(eventId);
                var dynamicPrice = await ApplyPricingAlgorithm(factors);
                
                _metricsService.RecordApiCall("CalculateDynamicPrice", 0.1, true);
                _logger.LogInformation("Dynamic price calculated for event {EventId}: {Price}", eventId, dynamicPrice);
                
                return dynamicPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating dynamic price for event {EventId}", eventId);
                _metricsService.RecordError("RevenueOptimization", ex.Message);
                throw;
            }
        }

        public async Task<PricingFactors> GetPricingFactors(int eventId)
        {
            var cacheKey = $"pricing_factors_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                    throw new ArgumentException($"Event {eventId} not found");

                var daysUntilEvent = (eventEntity.StartTime - DateTime.UtcNow).Days;
                var currentOccupancy = await GetOccupancyRate(eventId);
                var historicalDemand = await GetHistoricalDemand(eventEntity.Name);
                var competitorPrices = await GetCompetitorPrices(eventEntity.Name);
                var isHoliday = IsHoliday(eventEntity.StartTime);

                return new PricingFactors
                {
                    DaysUntilEvent = daysUntilEvent,
                    CurrentOccupancy = currentOccupancy,
                    DayOfWeek = eventEntity.StartTime.DayOfWeek,
                    HistoricalDemand = historicalDemand,
                    CompetitorPricing = competitorPrices,
                    EventType = eventEntity.Name,
                    EventCategory = eventEntity.Name,
                    IsHoliday = isHoliday,
                    BasePrice = 100000, // Base price in VND
                    MaxGuests = eventEntity.MaxGuests ?? 100,
                    CurrentRegistrations = await _guestRepository.GetCountByEventAsync(eventId)
                };
            }, TimeSpan.FromMinutes(5));
        }

        public async Task<decimal> GetHistoricalDemand(string eventType)
        {
            var cacheKey = $"historical_demand_{eventType}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate historical demand calculation
                var baseDemand = 0.8m; // 80% base demand
                var randomFactor = new Random().Next(60, 120) / 100.0m;
                return baseDemand * randomFactor;
            }, TimeSpan.FromHours(1));
        }

        public async Task<decimal[]> GetCompetitorPrices(string eventCategory)
        {
            var cacheKey = $"competitor_prices_{eventCategory}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate competitor pricing data
                return new decimal[]
                {
                    80000,  // Competitor 1
                    120000, // Competitor 2
                    95000,  // Competitor 3
                    110000  // Competitor 4
                };
            }, TimeSpan.FromHours(6));
        }

        public async Task<double> GetOccupancyRate(int eventId)
        {
            var cacheKey = $"occupancy_rate_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity?.MaxGuests == null || eventEntity.MaxGuests == 0)
                    return 0.0;

                var currentRegistrations = await _guestRepository.GetCountByEventAsync(eventId);
                return (double)currentRegistrations / eventEntity.MaxGuests.Value;
            }, TimeSpan.FromMinutes(2));
        }

        public async Task<OptimizationResult> OptimizeEventCapacity(int eventId)
        {
            try
            {
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                    throw new ArgumentException($"Event {eventId} not found");

                var historicalNoShowRate = await GetHistoricalNoShowRate(eventId);
                var weatherForecast = await GetWeatherImpact(eventId);
                var eventType = eventEntity.Name;

                // Calculate overbooking rate based on historical data
                var recommendedOverbooking = CalculateOverbookingRate(
                    historicalNoShowRate,
                    weatherForecast,
                    eventType
                );

                var baseCapacity = eventEntity.MaxGuests ?? 100;
                var recommendedCapacity = (int)(baseCapacity * (1 + recommendedOverbooking));
                var expectedAttendance = baseCapacity * (1 - historicalNoShowRate);

                var revenueImpact = await CalculateRevenueImpact(recommendedOverbooking, eventId);

                var result = new OptimizationResult
                {
                    RecommendedCapacity = recommendedCapacity,
                    ExpectedAttendance = expectedAttendance,
                    RevenueImpact = revenueImpact,
                    RecommendedOverbookingRate = recommendedOverbooking,
                    HistoricalNoShowRate = historicalNoShowRate,
                    WeatherForecast = weatherForecast,
                    Recommendation = GenerateRecommendation(recommendedOverbooking, historicalNoShowRate)
                };

                _logger.LogInformation("Capacity optimization completed for event {EventId}: {Recommendation}", 
                    eventId, result.Recommendation);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing capacity for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<RevenueImpact> CalculateRevenueImpact(double overbookingRate, int eventId)
        {
            var eventEntity = await _eventRepository.GetByIdAsync(eventId);
            if (eventEntity == null)
                throw new ArgumentException($"Event {eventId} not found");

            var baseCapacity = eventEntity.MaxGuests ?? 100;
            var currentRevenue = baseCapacity * 100000m; // Base price
            var additionalGuests = (int)(baseCapacity * overbookingRate);
            var optimizedRevenue = (baseCapacity + additionalGuests) * 100000m;
            var revenueIncrease = optimizedRevenue - currentRevenue;
            var percentageIncrease = (double)(revenueIncrease / currentRevenue * 100);

            return new RevenueImpact
            {
                CurrentRevenue = currentRevenue,
                OptimizedRevenue = optimizedRevenue,
                RevenueIncrease = revenueIncrease,
                PercentageIncrease = percentageIncrease,
                AdditionalGuests = additionalGuests
            };
        }

        private async Task<decimal> ApplyPricingAlgorithm(PricingFactors factors)
        {
            var basePrice = factors.BasePrice;
            var multiplier = 1.0m;

            // Days until event factor (urgency pricing)
            if (factors.DaysUntilEvent <= 7)
                multiplier *= 1.3m; // 30% increase for last week
            else if (factors.DaysUntilEvent <= 30)
                multiplier *= 1.1m; // 10% increase for last month

            // Occupancy factor (demand-based pricing)
            if (factors.CurrentOccupancy > 0.8)
                multiplier *= 1.2m; // 20% increase for high occupancy
            else if (factors.CurrentOccupancy < 0.3)
                multiplier *= 0.9m; // 10% discount for low occupancy

            // Day of week factor
            if (factors.DayOfWeek == DayOfWeek.Saturday || factors.DayOfWeek == DayOfWeek.Sunday)
                multiplier *= 1.15m; // 15% increase for weekends

            // Holiday factor
            if (factors.IsHoliday)
                multiplier *= 1.25m; // 25% increase for holidays

            // Historical demand factor
            multiplier *= factors.HistoricalDemand;

            // Competitor pricing factor
            if (factors.CompetitorPricing?.Length > 0)
            {
                var avgCompetitorPrice = factors.CompetitorPricing.Average();
                var competitorRatio = avgCompetitorPrice / basePrice;
                multiplier *= Math.Min(competitorRatio, 1.5m); // Cap at 50% above competitors
            }

            return Math.Round(basePrice * multiplier, -3); // Round to nearest 1000
        }

        private async Task<double> GetHistoricalNoShowRate(int eventId)
        {
            var cacheKey = $"no_show_rate_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate historical no-show rate calculation
                var baseRate = 0.15; // 15% base no-show rate
                var randomFactor = new Random().Next(80, 120) / 100.0;
                return baseRate * randomFactor;
            }, TimeSpan.FromHours(1));
        }

        private async Task<string> GetWeatherImpact(int eventId)
        {
            var cacheKey = $"weather_impact_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate weather forecast
                var weatherConditions = new[] { "Sunny", "Cloudy", "Rainy", "Stormy" };
                var random = new Random();
                return weatherConditions[random.Next(weatherConditions.Length)];
            }, TimeSpan.FromHours(1));
        }

        private double CalculateOverbookingRate(double noShowRate, string weather, string eventType)
        {
            var baseOverbooking = noShowRate * 0.8; // 80% of no-show rate

            // Weather adjustment
            if (weather == "Rainy" || weather == "Stormy")
                baseOverbooking *= 1.2; // 20% more overbooking for bad weather

            // Event type adjustment
            if (eventType.Contains("VIP") || eventType.Contains("Premium"))
                baseOverbooking *= 0.7; // 30% less overbooking for VIP events

            return Math.Min(baseOverbooking, 0.3); // Cap at 30% overbooking
        }

        private bool IsHoliday(DateTime date)
        {
            // Simple holiday check (can be enhanced with holiday API)
            var holidays = new[]
            {
                new DateTime(date.Year, 1, 1),   // New Year
                new DateTime(date.Year, 4, 30),  // Reunification Day
                new DateTime(date.Year, 5, 1),   // Labor Day
                new DateTime(date.Year, 9, 2),   // National Day
            };

            return holidays.Any(h => h.Date == date.Date);
        }

        private string GenerateRecommendation(double overbookingRate, double noShowRate)
        {
            if (overbookingRate > 0.2)
                return $"High overbooking recommended ({overbookingRate:P1}) due to high no-show rate ({noShowRate:P1})";
            else if (overbookingRate > 0.1)
                return $"Moderate overbooking recommended ({overbookingRate:P1})";
            else
                return $"Low overbooking recommended ({overbookingRate:P1}) due to low no-show rate ({noShowRate:P1})";
        }
    }
} 