using GiaNguyenCheck.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class CapacityOptimizer : ICapacityOptimizer
    {
        private readonly IEventRepository _eventRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly ICheckInRepository _checkInRepository;
        private readonly ICacheService _cacheService;
        private readonly IMetricsService _metricsService;
        private readonly ILogger<CapacityOptimizer> _logger;

        public CapacityOptimizer(
            IEventRepository eventRepository,
            IGuestRepository guestRepository,
            ICheckInRepository checkInRepository,
            ICacheService cacheService,
            IMetricsService metricsService,
            ILogger<CapacityOptimizer> logger)
        {
            _eventRepository = eventRepository;
            _guestRepository = guestRepository;
            _checkInRepository = checkInRepository;
            _cacheService = cacheService;
            _metricsService = metricsService;
            _logger = logger;
        }

        public async Task<OptimizationResult> OptimizeEventCapacity(int eventId)
        {
            try
            {
                _logger.LogInformation("Starting capacity optimization for event {EventId}", eventId);
                
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                    throw new ArgumentException($"Event {eventId} not found");

                var historicalNoShowRate = await GetHistoricalNoShowRate(eventId);
                var weatherForecast = await GetWeatherImpact(eventId);
                var eventType = eventEntity.Name;
                var baseCapacity = eventEntity.MaxGuests ?? 100;

                // Calculate overbooking strategy
                var overbookingStrategy = await CalculateOverbookingStrategy(eventId);
                var recommendedOverbooking = overbookingStrategy.RecommendedRate;

                // Calculate optimized capacity
                var recommendedCapacity = (int)(baseCapacity * (1 + recommendedOverbooking));
                var expectedAttendance = baseCapacity * (1 - historicalNoShowRate);

                // Calculate revenue impact
                var revenueImpact = await CalculateRevenueImpact(recommendedOverbooking, eventId);

                // Get capacity factors
                var factors = await GetCapacityFactors(eventId);

                var result = new OptimizationResult
                {
                    RecommendedCapacity = recommendedCapacity,
                    ExpectedAttendance = expectedAttendance,
                    RevenueImpact = revenueImpact,
                    RecommendedOverbookingRate = recommendedOverbooking,
                    HistoricalNoShowRate = historicalNoShowRate,
                    WeatherForecast = weatherForecast,
                    Recommendation = GenerateRecommendation(recommendedOverbooking, historicalNoShowRate, factors),
                    Factors = factors,
                    Strategy = overbookingStrategy
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

        public async Task<double> GetHistoricalNoShowRate(int eventId)
        {
            var cacheKey = $"no_show_rate_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Get historical data for similar events
                var similarEvents = await GetSimilarEvents(eventId);
                var totalInvited = 0;
                var totalAttended = 0;

                foreach (var similarEvent in similarEvents)
                {
                    var invited = await _guestRepository.GetCountByEventAsync(similarEvent.Id);
                    var attended = await _checkInRepository.GetCountByEventAsync(similarEvent.Id);
                    totalInvited += invited;
                    totalAttended += attended;
                }

                var noShowRate = totalInvited > 0 ? 1.0 - ((double)totalAttended / totalInvited) : 0.15;
                
                // Apply seasonal adjustments
                noShowRate = ApplySeasonalAdjustments(noShowRate, DateTime.UtcNow);
                
                return Math.Min(noShowRate, 0.4); // Cap at 40%
            }, TimeSpan.FromHours(2));
        }

        public async Task<string> GetWeatherImpact(int eventId)
        {
            var cacheKey = $"weather_impact_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                // Simulate weather forecast API call
                var weatherConditions = new[] { "Sunny", "Cloudy", "Rainy", "Stormy", "Windy" };
                var random = new Random();
                return weatherConditions[random.Next(weatherConditions.Length)];
            }, TimeSpan.FromHours(1));
        }

        public async Task<CapacityRecommendation> GetCapacityRecommendation(int eventId)
        {
            try
            {
                var optimizationResult = await OptimizeEventCapacity(eventId);
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                
                var baseCapacity = eventEntity?.MaxGuests ?? 100;
                var confidenceLevel = CalculateConfidenceLevel(optimizationResult.Factors);
                var warnings = GenerateWarnings(optimizationResult);

                var recommendation = new CapacityRecommendation
                {
                    BaseCapacity = baseCapacity,
                    OptimizedCapacity = optimizationResult.RecommendedCapacity,
                    OverbookingRate = optimizationResult.RecommendedOverbookingRate,
                    ExpectedNoShowRate = optimizationResult.HistoricalNoShowRate,
                    ConfidenceLevel = confidenceLevel,
                    Reasoning = optimizationResult.Recommendation,
                    Warnings = warnings
                };

                _metricsService.RecordApiCall("GetCapacityRecommendation", 0.1, true);
                return recommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting capacity recommendation for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<OverbookingStrategy> CalculateOverbookingStrategy(int eventId)
        {
            var cacheKey = $"overbooking_strategy_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                var historicalNoShowRate = await GetHistoricalNoShowRate(eventId);
                var weatherForecast = await GetWeatherImpact(eventId);
                var eventType = eventEntity?.Name ?? "Unknown";
                var dayOfWeek = eventEntity?.StartTime.DayOfWeek ?? DayOfWeek.Monday;

                // Base overbooking rates
                var conservativeRate = historicalNoShowRate * 0.7; // 70% of no-show rate
                var moderateRate = historicalNoShowRate * 0.9;     // 90% of no-show rate
                var aggressiveRate = historicalNoShowRate * 1.2;   // 120% of no-show rate

                // Apply adjustments based on factors
                var riskFactors = new Dictionary<string, double>
                {
                    { "Weather", GetWeatherAdjustment(weatherForecast) },
                    { "DayOfWeek", GetDayOfWeekAdjustment(dayOfWeek) },
                    { "EventType", GetEventTypeAdjustment(eventType) },
                    { "Season", GetSeasonalAdjustment(DateTime.UtcNow) },
                    { "HistoricalAccuracy", GetHistoricalAccuracyAdjustment(eventId) }
                };

                // Calculate recommended rate
                var adjustmentFactor = riskFactors.Values.Average();
                var recommendedRate = moderateRate * adjustmentFactor;

                // Determine strategy type
                var strategyType = recommendedRate switch
                {
                    > 0.25 => "Aggressive",
                    > 0.15 => "Moderate",
                    _ => "Conservative"
                };

                return new OverbookingStrategy
                {
                    ConservativeRate = conservativeRate,
                    ModerateRate = moderateRate,
                    AggressiveRate = aggressiveRate,
                    RecommendedRate = Math.Min(recommendedRate, 0.3), // Cap at 30%
                    StrategyType = strategyType,
                    RiskFactors = riskFactors
                };
            }, TimeSpan.FromHours(1));
        }

        public async Task<RevenueImpact> CalculateRevenueImpact(double overbookingRate, int eventId)
        {
            try
            {
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                if (eventEntity == null)
                    throw new ArgumentException($"Event {eventId} not found");

                var baseCapacity = eventEntity.MaxGuests ?? 100;
                var ticketPrice = 100000m; // Base ticket price in VND
                var costPerGuest = 20000m; // Operational cost per guest

                var currentRevenue = baseCapacity * ticketPrice;
                var additionalGuests = (int)(baseCapacity * overbookingRate);
                var optimizedRevenue = (baseCapacity + additionalGuests) * ticketPrice;
                var revenueIncrease = optimizedRevenue - currentRevenue;
                var percentageIncrease = (double)(revenueIncrease / currentRevenue * 100);
                var netProfitIncrease = revenueIncrease - (additionalGuests * costPerGuest);

                var impact = new RevenueImpact
                {
                    CurrentRevenue = currentRevenue,
                    OptimizedRevenue = optimizedRevenue,
                    RevenueIncrease = revenueIncrease,
                    PercentageIncrease = percentageIncrease,
                    AdditionalGuests = additionalGuests,
                    CostPerGuest = costPerGuest,
                    NetProfitIncrease = netProfitIncrease
                };

                _metricsService.RecordApiCall("CalculateRevenueImpact", 0.05, true);
                return impact;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue impact for event {EventId}", eventId);
                throw;
            }
        }

        public async Task<List<CapacityFactor>> GetCapacityFactors(int eventId)
        {
            var cacheKey = $"capacity_factors_{eventId}";
            return await _cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var factors = new List<CapacityFactor>();

                // Historical no-show rate factor
                var noShowRate = await GetHistoricalNoShowRate(eventId);
                factors.Add(new CapacityFactor
                {
                    Name = "Historical No-Show Rate",
                    Weight = 0.3,
                    Description = $"Based on {noShowRate:P1} historical no-show rate",
                    Impact = noShowRate,
                    IsPositive = noShowRate > 0.1 // Higher no-show rate allows more overbooking
                });

                // Weather factor
                var weather = await GetWeatherImpact(eventId);
                var weatherImpact = GetWeatherImpactValue(weather);
                factors.Add(new CapacityFactor
                {
                    Name = "Weather Forecast",
                    Weight = 0.2,
                    Description = $"Weather: {weather}",
                    Impact = weatherImpact,
                    IsPositive = weatherImpact > 0.5
                });

                // Event type factor
                var eventEntity = await _eventRepository.GetByIdAsync(eventId);
                var eventTypeImpact = GetEventTypeImpact(eventEntity?.Name);
                factors.Add(new CapacityFactor
                {
                    Name = "Event Type",
                    Weight = 0.25,
                    Description = $"Event type: {eventEntity?.Name}",
                    Impact = eventTypeImpact,
                    IsPositive = eventTypeImpact > 0.5
                });

                // Day of week factor
                var dayOfWeek = eventEntity?.StartTime.DayOfWeek ?? DayOfWeek.Monday;
                var dayImpact = GetDayOfWeekImpact(dayOfWeek);
                factors.Add(new CapacityFactor
                {
                    Name = "Day of Week",
                    Weight = 0.15,
                    Description = $"Event on {dayOfWeek}",
                    Impact = dayImpact,
                    IsPositive = dayImpact > 0.5
                });

                // Seasonal factor
                var seasonalImpact = GetSeasonalImpact(DateTime.UtcNow);
                factors.Add(new CapacityFactor
                {
                    Name = "Seasonal Factor",
                    Weight = 0.1,
                    Description = $"Season: {GetCurrentSeason(DateTime.UtcNow)}",
                    Impact = seasonalImpact,
                    IsPositive = seasonalImpact > 0.5
                });

                return factors;
            }, TimeSpan.FromHours(1));
        }

        private string GenerateRecommendation(double overbookingRate, double noShowRate, List<CapacityFactor> factors)
        {
            var positiveFactors = factors.Where(f => f.IsPositive).Count();
            var totalFactors = factors.Count;
            var confidence = (double)positiveFactors / totalFactors;

            if (overbookingRate > 0.25)
                return $"High overbooking recommended ({overbookingRate:P1}) - {confidence:P0} confidence based on {positiveFactors}/{totalFactors} positive factors";
            else if (overbookingRate > 0.15)
                return $"Moderate overbooking recommended ({overbookingRate:P1}) - {confidence:P0} confidence";
            else if (overbookingRate > 0.05)
                return $"Low overbooking recommended ({overbookingRate:P1}) - {confidence:P0} confidence";
            else
                return $"No overbooking recommended - {confidence:P0} confidence";
        }

        private double CalculateConfidenceLevel(List<CapacityFactor> factors)
        {
            if (!factors.Any()) return 0.5;

            var weightedSum = factors.Sum(f => f.Weight * (f.IsPositive ? 1.0 : 0.0));
            var totalWeight = factors.Sum(f => f.Weight);
            
            return totalWeight > 0 ? weightedSum / totalWeight : 0.5;
        }

        private List<string> GenerateWarnings(OptimizationResult result)
        {
            var warnings = new List<string>();

            if (result.RecommendedOverbookingRate > 0.25)
                warnings.Add("High overbooking rate may lead to capacity issues");

            if (result.HistoricalNoShowRate < 0.05)
                warnings.Add("Low historical no-show rate - overbooking may be risky");

            if (result.Factors.Any(f => !f.IsPositive && f.Weight > 0.2))
                warnings.Add("Several negative factors detected - consider conservative approach");

            if (result.WeatherForecast == "Stormy" || result.WeatherForecast == "Rainy")
                warnings.Add("Bad weather forecast may affect attendance");

            return warnings;
        }

        private double ApplySeasonalAdjustments(double noShowRate, DateTime date)
        {
            var month = date.Month;
            
            // Holiday season adjustments
            if (month == 12 || month == 1) // December/January
                noShowRate *= 1.2; // 20% higher during holidays
            else if (month == 6 || month == 7) // Summer months
                noShowRate *= 1.1; // 10% higher during summer
            else if (month == 9) // September (back to school)
                noShowRate *= 0.9; // 10% lower

            return noShowRate;
        }

        private double GetWeatherAdjustment(string weather)
        {
            return weather switch
            {
                "Sunny" => 1.0,
                "Cloudy" => 0.95,
                "Windy" => 0.9,
                "Rainy" => 0.8,
                "Stormy" => 0.7,
                _ => 1.0
            };
        }

        private double GetDayOfWeekAdjustment(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Friday => 1.1,   // Higher attendance
                DayOfWeek.Saturday => 1.2, // Highest attendance
                DayOfWeek.Sunday => 1.1,   // High attendance
                DayOfWeek.Monday => 0.9,   // Lower attendance
                DayOfWeek.Tuesday => 0.95, // Slightly lower
                DayOfWeek.Wednesday => 1.0, // Normal
                DayOfWeek.Thursday => 1.05, // Slightly higher
                _ => 1.0
            };
        }

        private double GetEventTypeAdjustment(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return 1.0;

            return eventType.ToLower() switch
            {
                var type when type.Contains("vip") => 0.8,      // VIP events - lower no-show
                var type when type.Contains("premium") => 0.85, // Premium events
                var type when type.Contains("free") => 1.3,     // Free events - higher no-show
                var type when type.Contains("workshop") => 0.9, // Workshops
                var type when type.Contains("conference") => 0.95, // Conferences
                _ => 1.0
            };
        }

        private double GetSeasonalAdjustment(DateTime date)
        {
            var month = date.Month;
            
            return month switch
            {
                12 or 1 => 1.2,  // Holiday season
                6 or 7 => 1.1,   // Summer
                9 => 0.9,         // Back to school
                _ => 1.0
            };
        }

        private double GetHistoricalAccuracyAdjustment(int eventId)
        {
            // Simulate historical accuracy based on event ID
            var random = new Random(eventId);
            return 0.8 + (random.NextDouble() * 0.4); // 0.8 to 1.2
        }

        private double GetWeatherImpactValue(string weather)
        {
            return weather switch
            {
                "Sunny" => 0.8,   // Good weather - lower no-show
                "Cloudy" => 0.9,  // Decent weather
                "Windy" => 1.0,   // Neutral
                "Rainy" => 1.2,   // Bad weather - higher no-show
                "Stormy" => 1.4,  // Very bad weather
                _ => 1.0
            };
        }

        private double GetEventTypeImpact(string eventType)
        {
            if (string.IsNullOrEmpty(eventType)) return 0.5;

            return eventType.ToLower() switch
            {
                var type when type.Contains("vip") => 0.3,      // VIP - very low no-show
                var type when type.Contains("premium") => 0.4,  // Premium - low no-show
                var type when type.Contains("free") => 0.7,     // Free - high no-show
                var type when type.Contains("workshop") => 0.5, // Workshop - medium
                var type when type.Contains("conference") => 0.6, // Conference - medium-high
                _ => 0.5
            };
        }

        private double GetDayOfWeekImpact(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Saturday => 0.3,  // Best attendance
                DayOfWeek.Sunday => 0.4,    // Good attendance
                DayOfWeek.Friday => 0.5,    // Good attendance
                DayOfWeek.Thursday => 0.6,  // Medium attendance
                DayOfWeek.Wednesday => 0.7, // Medium attendance
                DayOfWeek.Tuesday => 0.8,   // Lower attendance
                DayOfWeek.Monday => 0.9,    // Lowest attendance
                _ => 0.5
            };
        }

        private double GetSeasonalImpact(DateTime date)
        {
            var month = date.Month;
            
            return month switch
            {
                12 or 1 => 0.6,  // Holiday season - higher no-show
                6 or 7 => 0.7,   // Summer - higher no-show
                9 => 0.4,        // Back to school - lower no-show
                _ => 0.5
            };
        }

        private string GetCurrentSeason(DateTime date)
        {
            var month = date.Month;
            
            return month switch
            {
                12 or 1 or 2 => "Winter",
                3 or 4 or 5 => "Spring",
                6 or 7 or 8 => "Summer",
                _ => "Fall"
            };
        }

        // Stub methods for data access
        private async Task<List<dynamic>> GetSimilarEvents(int eventId) => new List<dynamic>();
    }
} 