using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Histogram;
using App.Metrics.Gauge;
using App.Metrics.Timer;
using Microsoft.Extensions.Logging;
using System;

namespace GiaNguyenCheck.Services
{
    public class MetricsService : IMetricsService
    {
        private readonly IMetrics _metrics;
        private readonly ILogger<MetricsService> _logger;

        public MetricsService(IMetrics metrics, ILogger<MetricsService> logger)
        {
            _metrics = metrics;
            _logger = logger;
        }

        public void RecordCheckIn(string eventType, double duration)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "checkins_total" },
                    new MetricTags("event_type", eventType));

                _metrics.Measure.Histogram.Update(
                    new HistogramOptions { Name = "checkin_duration_seconds" },
                    duration);

                _logger.LogDebug("Recorded check-in metrics: EventType={EventType}, Duration={Duration}s", eventType, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording check-in metrics");
            }
        }

        public void RecordPayment(string method, decimal amount, bool success)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "payments_total" },
                    new MetricTags(
                        new[] { "method", "status" },
                        new[] { method, success ? "success" : "failed" }));

                if (success)
                {
                    _metrics.Measure.Gauge.SetValue(
                        new GaugeOptions { Name = "payment_amount_total" },
                        Convert.ToDouble(amount));
                }

                _logger.LogDebug("Recorded payment metrics: Method={Method}, Amount={Amount}, Success={Success}", method, amount, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording payment metrics");
            }
        }

        public void RecordEventCreation(string eventType)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "events_created_total" },
                    new MetricTags("event_type", eventType));

                _logger.LogDebug("Recorded event creation metrics: EventType={EventType}", eventType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording event creation metrics");
            }
        }

        public void RecordGuestRegistration(string guestType)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "guests_registered_total" },
                    new MetricTags("guest_type", guestType));

                _logger.LogDebug("Recorded guest registration metrics: GuestType={GuestType}", guestType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording guest registration metrics");
            }
        }

        public void RecordEmailSent(string emailType, bool success)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "emails_sent_total" },
                    new MetricTags(
                        new[] { "email_type", "status" },
                        new[] { emailType, success ? "success" : "failed" }));

                _logger.LogDebug("Recorded email metrics: EmailType={EmailType}, Success={Success}", emailType, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording email metrics");
            }
        }

        public void RecordApiCall(string endpoint, double duration, bool success)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "api_calls_total" },
                    new MetricTags(
                        new[] { "endpoint", "status" },
                        new[] { endpoint, success ? "success" : "failed" }));

                _metrics.Measure.Histogram.Update(
                    new HistogramOptions { Name = "api_call_duration_seconds" },
                    duration);

                _logger.LogDebug("Recorded API call metrics: Endpoint={Endpoint}, Duration={Duration}s, Success={Success}", endpoint, duration, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording API call metrics");
            }
        }

        public void RecordDatabaseQuery(string queryType, double duration)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "database_queries_total" },
                    new MetricTags("query_type", queryType));

                _metrics.Measure.Histogram.Update(
                    new HistogramOptions { Name = "database_query_duration_seconds" },
                    duration);

                _logger.LogDebug("Recorded database query metrics: QueryType={QueryType}, Duration={Duration}s", queryType, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording database query metrics");
            }
        }

        public void RecordCacheHit(string cacheType, string key)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "cache_hits_total" },
                    new MetricTags("cache_type", cacheType));

                _logger.LogDebug("Recorded cache hit metrics: CacheType={CacheType}, Key={Key}", cacheType, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache hit metrics");
            }
        }

        public void RecordCacheMiss(string cacheType, string key)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "cache_misses_total" },
                    new MetricTags("cache_type", cacheType));

                _logger.LogDebug("Recorded cache miss metrics: CacheType={CacheType}, Key={Key}", cacheType, key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache miss metrics");
            }
        }

        public void RecordError(string errorType, string message)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "errors_total" },
                    new MetricTags("error_type", errorType));

                _logger.LogDebug("Recorded error metrics: ErrorType={ErrorType}, Message={Message}", errorType, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording error metrics");
            }
        }

        public void RecordTenantActivity(int tenantId, string activity)
        {
            try
            {
                _metrics.Measure.Counter.Increment(
                    new CounterOptions { Name = "tenant_activities_total" },
                    new MetricTags(
                        new[] { "tenant_id", "activity" },
                        new[] { tenantId.ToString(), activity }));

                _logger.LogDebug("Recorded tenant activity metrics: TenantId={TenantId}, Activity={Activity}", tenantId, activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording tenant activity metrics");
            }
        }
    }
} 