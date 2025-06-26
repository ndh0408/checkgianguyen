using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services.HealthChecks
{
    public class PaymentGatewayHealthCheck : IHealthCheck
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentGatewayHealthCheck> _logger;
        private readonly IConfiguration _configuration;

        public PaymentGatewayHealthCheck(
            HttpClient httpClient,
            ILogger<PaymentGatewayHealthCheck> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var results = new List<(string Gateway, bool IsHealthy, string Message)>();

                // Check MoMo API
                var momoHealthy = await CheckMoMoHealth(cancellationToken);
                results.Add(("MoMo", momoHealthy.IsHealthy, momoHealthy.Description));

                // Check VNPAY API
                var vnpayHealthy = await CheckVNPAYHealth(cancellationToken);
                results.Add(("VNPAY", vnpayHealthy.IsHealthy, vnpayHealthy.Description));

                // Check Stripe API
                var stripeHealthy = await CheckStripeHealth(cancellationToken);
                results.Add(("Stripe", stripeHealthy.IsHealthy, stripeHealthy.Description));

                // Check PayPal API
                var paypalHealthy = await CheckPayPalHealth(cancellationToken);
                results.Add(("PayPal", paypalHealthy.IsHealthy, paypalHealthy.Description));

                var healthyCount = results.Count(r => r.IsHealthy);
                var totalCount = results.Count;

                if (healthyCount == totalCount)
                {
                    return HealthCheckResult.Healthy(
                        "All payment gateways are operational",
                        new Dictionary<string, object>
                        {
                            { "healthy_gateways", healthyCount },
                            { "total_gateways", totalCount },
                            { "gateway_status", results.ToDictionary(r => r.Gateway, r => r.Message) }
                        });
                }
                else if (healthyCount > 0)
                {
                    return HealthCheckResult.Degraded(
                        $"Some payment gateways are down. {healthyCount}/{totalCount} operational",
                        new Dictionary<string, object>
                        {
                            { "healthy_gateways", healthyCount },
                            { "total_gateways", totalCount },
                            { "gateway_status", results.ToDictionary(r => r.Gateway, r => r.Message) }
                        });
                }
                else
                {
                    return HealthCheckResult.Unhealthy(
                        "All payment gateways are down",
                        new Dictionary<string, object>
                        {
                            { "healthy_gateways", healthyCount },
                            { "total_gateways", totalCount },
                            { "gateway_status", results.ToDictionary(r => r.Gateway, r => r.Message) }
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment gateway health check failed");
                return HealthCheckResult.Unhealthy("Payment gateway check failed", ex);
            }
        }

        private async Task<HealthCheckResult> CheckMoMoHealth(CancellationToken cancellationToken)
        {
            try
            {
                var endpoint = _configuration["PaymentSettings:MoMo:Endpoint"];
                if (string.IsNullOrEmpty(endpoint))
                {
                    return HealthCheckResult.Unhealthy("MoMo endpoint not configured");
                }

                // Simple ping to MoMo API
                var response = await _httpClient.GetAsync($"{endpoint}/health", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("MoMo API is operational");
                }

                return HealthCheckResult.Unhealthy($"MoMo API returned status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MoMo health check failed");
                return HealthCheckResult.Unhealthy("MoMo API check failed", ex);
            }
        }

        private async Task<HealthCheckResult> CheckVNPAYHealth(CancellationToken cancellationToken)
        {
            try
            {
                var endpoint = _configuration["PaymentSettings:VNPAY:Url"];
                if (string.IsNullOrEmpty(endpoint))
                {
                    return HealthCheckResult.Unhealthy("VNPAY endpoint not configured");
                }

                // Simple ping to VNPAY API
                var response = await _httpClient.GetAsync($"{endpoint}/health", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("VNPAY API is operational");
                }

                return HealthCheckResult.Unhealthy($"VNPAY API returned status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "VNPAY health check failed");
                return HealthCheckResult.Unhealthy("VNPAY API check failed", ex);
            }
        }

        private async Task<HealthCheckResult> CheckStripeHealth(CancellationToken cancellationToken)
        {
            try
            {
                var secretKey = _configuration["PaymentSettings:Stripe:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    return HealthCheckResult.Unhealthy("Stripe secret key not configured");
                }

                // Check Stripe API with a simple balance request
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secretKey);

                var response = await _httpClient.GetAsync("https://api.stripe.com/v1/balance", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Healthy("Stripe API is operational");
                }

                return HealthCheckResult.Unhealthy($"Stripe API returned status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Stripe health check failed");
                return HealthCheckResult.Unhealthy("Stripe API check failed", ex);
            }
        }

        private async Task<HealthCheckResult> CheckPayPalHealth(CancellationToken cancellationToken)
        {
            try
            {
                // PayPal health check - simple ping to PayPal API
                var response = await _httpClient.GetAsync("https://api.paypal.com/v1/identity/openidconnect/userinfo", cancellationToken);
                if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // 401 is expected without proper authentication, but means API is reachable
                    return HealthCheckResult.Healthy("PayPal API is operational");
                }

                return HealthCheckResult.Unhealthy($"PayPal API returned status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PayPal health check failed");
                return HealthCheckResult.Unhealthy("PayPal API check failed", ex);
            }
        }
    }
} 