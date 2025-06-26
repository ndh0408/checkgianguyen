using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, bool> _featureFlags;

        public FeatureService(IConfiguration configuration)
        {
            _configuration = configuration;
            _featureFlags = new Dictionary<string, bool>
            {
                // Default feature flags
                { "BulkCheckIn", true },
                { "QRCodeEncryption", true },
                { "RealTimeDashboard", true },
                { "PaymentIntegration", true },
                { "EmailAutomation", true },
                { "AdvancedAnalytics", false },
                { "MobileApp", false },
                { "AIFeatures", false }
            };
        }

        public bool IsEnabled(string feature, int? tenantId = null)
        {
            // Check tenant-specific feature flag first
            if (tenantId.HasValue)
            {
                var tenantKey = $"Features:Tenant_{tenantId}:{feature}";
                var tenantValue = _configuration[tenantKey];
                if (!string.IsNullOrEmpty(tenantValue))
                {
                    return bool.Parse(tenantValue);
                }
            }

            // Check global feature flag
            var globalKey = $"Features:{feature}";
            var globalValue = _configuration[globalKey];
            if (!string.IsNullOrEmpty(globalValue))
            {
                return bool.Parse(globalValue);
            }

            // Return default value
            return _featureFlags.TryGetValue(feature, out bool defaultValue) && defaultValue;
        }

        public async Task<bool> IsEnabledAsync(string feature, int? tenantId = null)
        {
            return await Task.FromResult(IsEnabled(feature, tenantId));
        }

        public void EnableFeature(string feature, int? tenantId = null)
        {
            // In production, this would update configuration or database
            if (tenantId.HasValue)
            {
                // Enable for specific tenant
                _featureFlags[$"Tenant_{tenantId}_{feature}"] = true;
            }
            else
            {
                // Enable globally
                _featureFlags[feature] = true;
            }
        }

        public void DisableFeature(string feature, int? tenantId = null)
        {
            // In production, this would update configuration or database
            if (tenantId.HasValue)
            {
                // Disable for specific tenant
                _featureFlags[$"Tenant_{tenantId}_{feature}"] = false;
            }
            else
            {
                // Disable globally
                _featureFlags[feature] = false;
            }
        }

        public async Task<bool> IsFeatureEnabledForUserAsync(string feature, int userId)
        {
            // In production, check user-specific feature flags
            // For now, return the global setting
            return await IsEnabledAsync(feature);
        }
    }
} 