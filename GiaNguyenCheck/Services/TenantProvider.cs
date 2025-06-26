using GiaNguyenCheck.Constants;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using GiaNguyenCheck.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GiaNguyenCheck.Services
{
    /// <summary>
    /// Service cung cấp thông tin tenant hiện tại trong context
    /// </summary>
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantRepository _tenantRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TenantProvider> _logger;
        private const string TenantIdKey = "TenantId";
        private const string SubdomainKey = "TenantSubdomain";

        public TenantProvider(IHttpContextAccessor httpContextAccessor,
                              ITenantRepository tenantRepository,
                              IServiceProvider serviceProvider,
                              ILogger<TenantProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantRepository = tenantRepository;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Lấy ID tenant hiện tại
        /// </summary>
        public int GetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Items.ContainsKey(TenantIdKey))
            {
                return (int)context.Items[TenantIdKey]!;
            }
            return 0;
        }

        /// <summary>
        /// Kiểm tra xem có tenant nào được set hay không
        /// </summary>
        public bool HasTenant()
        {
            return GetTenantId() != 0;
        }

        /// <summary>
        /// Set tenant ID cho request hiện tại
        /// </summary>
        public async Task SetTenantIdAsync(int tenantId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items[TenantIdKey] = tenantId;
            }
        }

        /// <summary>
        /// Set tenant ID (đồng bộ) – dùng cho middleware
        /// </summary>
        public void SetTenantId(int tenantId)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                context.Items[TenantIdKey] = tenantId;
            }
        }

        /// <summary>
        /// Xóa tenant ID khỏi context hiện tại
        /// </summary>
        public void ClearTenant()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Items.ContainsKey(TenantIdKey))
            {
                context.Items.Remove(TenantIdKey);
            }
        }

        /// <summary>
        /// Lấy thông tin tenant hiện tại
        /// </summary>
        public async Task<Tenant?> GetCurrentTenantAsync()
        {
            var tenantId = GetTenantId();
            if (tenantId == 0)
                return null;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                
                return await dbContext.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin tenant {TenantId}", tenantId);
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra xem user hiện tại có quyền truy cập resource của tenant không
        /// </summary>
        public async Task<bool> CanAccessResourceAsync(int resourceTenantId)
        {
            var currentTenantId = GetTenantId();
            
            // System Admin có thể truy cập tất cả
            if (IsSystemAdmin())
                return true;
                
            // Chỉ được truy cập resource của tenant mình
            return currentTenantId == resourceTenantId;
        }

        /// <summary>
        /// Kiểm tra xem tenant hiện tại có được phép thực hiện action này không
        /// </summary>
        public async Task<bool> CanPerformActionAsync(string action)
        {
            var tenant = await GetCurrentTenantAsync();
            if (tenant == null)
                return false;

            // Kiểm tra service limits dựa vào plan
            switch (action)
            {
                case "CreateEvent":
                    var eventCount = await GetEventCountAsync(tenant.Id);
                    return eventCount < GetMaxEvents(tenant.CurrentPlan);
                    
                case "AddGuest":
                    var guestCount = await GetGuestCountAsync(tenant.Id);
                    return guestCount < GetMaxGuests(tenant.CurrentPlan);
                    
                default:
                    return true;
            }
        }

        /// <summary>
        /// Lấy user ID hiện tại
        /// </summary>
        public int? GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            
            if (user?.Identity?.IsAuthenticated != true)
                return null;
                
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            
            return null;
        }

        /// <summary>
        /// Lấy role của user hiện tại
        /// </summary>
        public string? GetCurrentUserRole()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;
            
            if (user?.Identity?.IsAuthenticated != true)
                return null;
                
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Kiểm tra xem có phải là System Admin không
        /// </summary>
        private bool IsSystemAdmin()
        {
            var role = GetCurrentUserRole();
            return role == AppConstants.Roles.SystemAdmin;
        }

        /// <summary>
        /// Lấy số lượng events hiện tại của tenant
        /// </summary>
        private async Task<int> GetEventCountAsync(int tenantId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                
                return await dbContext.Events
                    .Where(e => e.TenantId == tenantId)
                    .CountAsync();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lấy số lượng guests hiện tại của tenant
        /// </summary>
        private async Task<int> GetGuestCountAsync(int tenantId)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                
                return await dbContext.Guests
                    .Where(g => g.TenantId == tenantId)
                    .CountAsync();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Lấy giới hạn số events theo plan
        /// </summary>
        private int GetMaxEvents(ServicePlan plan)
        {
            return plan switch
            {
                ServicePlan.Free => AppConstants.ServiceLimits.Free.MaxEvents,
                ServicePlan.Basic => AppConstants.ServiceLimits.Basic.MaxEvents,
                ServicePlan.Pro => AppConstants.ServiceLimits.Pro.MaxEvents,
                ServicePlan.Enterprise => int.MaxValue,
                _ => 0
            };
        }

        /// <summary>
        /// Lấy giới hạn số guests theo plan
        /// </summary>
        private int GetMaxGuests(ServicePlan plan)
        {
            return plan switch
            {
                ServicePlan.Free => AppConstants.ServiceLimits.Free.MaxGuests,
                ServicePlan.Basic => AppConstants.ServiceLimits.Basic.MaxGuests,
                ServicePlan.Pro => AppConstants.ServiceLimits.Pro.MaxGuests,
                ServicePlan.Enterprise => int.MaxValue,
                _ => 0
            };
        }

        public async Task SetTenantBySubdomainAsync(string subdomain)
        {
            var tenant = await _tenantRepository.GetBySubdomainAsync(subdomain);
            if (tenant != null)
            {
                await SetTenantIdAsync(tenant.Id);
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Items[SubdomainKey] = subdomain;
                }
            }
        }

        public string? GetCurrentSubdomain()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Items.ContainsKey(SubdomainKey))
            {
                return context.Items[SubdomainKey]?.ToString();
            }
            return null;
        }

        public int GetCurrentTenantId()
        {
            return GetTenantId();
        }
    }
} 