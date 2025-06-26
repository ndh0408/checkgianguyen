using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface IFeatureService
    {
        bool IsEnabled(string feature, int? tenantId = null);
        Task<bool> IsEnabledAsync(string feature, int? tenantId = null);
        void EnableFeature(string feature, int? tenantId = null);
        void DisableFeature(string feature, int? tenantId = null);
        Task<bool> IsFeatureEnabledForUserAsync(string feature, int userId);
    }
} 