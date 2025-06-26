using GiaNguyenCheck.Constants;
using GiaNguyenCheck.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Middlewares
{
    /// <summary>
    /// Middleware xử lý multi-tenant cho mỗi request
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;
        
        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            try
            {
                await ResolveTenantAsync(context, tenantProvider);
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong TenantMiddleware");
                throw;
            }
        }
        
        /// <summary>
        /// Xác định tenant từ các nguồn khác nhau
        /// </summary>
        private async Task ResolveTenantAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            int tenantId = 0;
            
            // 1. Ưu tiên: Lấy từ JWT claims (người dùng đã đăng nhập)
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantClaim = context.User.FindFirst(AppConstants.Claims.TenantId);
                if (tenantClaim != null && int.TryParse(tenantClaim.Value, out var claimTenantId))
                {
                    tenantId = claimTenantId;
                    _logger.LogDebug("Tenant được xác định từ JWT Claims: {TenantId}", tenantId);
                }
            }
            
            // 2. Lấy từ header (cho API calls)
            if (tenantId == 0)
            {
                if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
                {
                    if (int.TryParse(headerTenantId.FirstOrDefault(), out var parsedTenantId))
                    {
                        tenantId = parsedTenantId;
                        _logger.LogDebug("Tenant được xác định từ Header: {TenantId}", tenantId);
                    }
                }
            }
            
            // 3. Lấy từ subdomain (cho white-label solutions)
            if (tenantId == 0)
            {
                tenantId = await ResolveTenantFromSubdomainAsync(context);
                if (tenantId != 0)
                {
                    _logger.LogDebug("Tenant được xác định từ Subdomain: {TenantId}", tenantId);
                }
            }
            
            // 4. Lấy từ query parameter (backup method)
            if (tenantId == 0)
            {
                if (context.Request.Query.TryGetValue("tenantId", out var queryTenantId))
                {
                    if (int.TryParse(queryTenantId.FirstOrDefault(), out var parsedTenantId))
                    {
                        tenantId = parsedTenantId;
                        _logger.LogDebug("Tenant được xác định từ Query Parameter: {TenantId}", tenantId);
                    }
                }
            }
            
            // Set tenant vào provider
            if (tenantId != 0)
            {
                tenantProvider.SetTenantId(tenantId);
                
                // Thêm vào HttpContext.Items để sử dụng trong các service khác
                context.Items["TenantId"] = tenantId;
                
                _logger.LogInformation("Tenant được set thành công: {TenantId}", tenantId);
            }
            else
            {
                // Kiểm tra xem có phải là System Admin không
                var roleClaim = context.User.FindFirst(AppConstants.Claims.Role);
                if (roleClaim?.Value == AppConstants.Roles.SystemAdmin)
                {
                    _logger.LogDebug("System Admin được xác định, không cần tenant");
                }
                else if (IsProtectedEndpoint(context))
                {
                    _logger.LogWarning("Không thể xác định tenant cho protected endpoint: {Path}", context.Request.Path);
                }
            }
            
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Xác định tenant từ subdomain
        /// </summary>
        private async Task<int> ResolveTenantFromSubdomainAsync(HttpContext context)
        {
            try
            {
                var host = context.Request.Host.Host;
                
                // Bỏ qua localhost và IP addresses
                if (host == "localhost" || System.Net.IPAddress.TryParse(host, out _))
                {
                    return 0;
                }
                
                // Tách subdomain
                var parts = host.Split('.');
                if (parts.Length >= 3) // subdomain.domain.com
                {
                    var subdomain = parts[0];
                    
                    // TODO: Implement logic để map subdomain -> tenant
                    // Có thể cache mapping này để tăng hiệu suất
                    
                    _logger.LogDebug("Subdomain được phát hiện: {Subdomain}", subdomain);
                    
                    // Placeholder - trong thực tế cần query database
                    // return await _tenantRepository.GetBySubdomainAsync(subdomain);
                }
                
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi resolve tenant từ subdomain");
                return 0;
            }
        }
        
        /// <summary>
        /// Kiểm tra xem endpoint có cần tenant không
        /// </summary>
        private bool IsProtectedEndpoint(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();
            
            if (string.IsNullOrEmpty(path))
                return false;
                
            // Các endpoint không cần tenant
            var publicPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/forgot-password",
                "/api/auth/reset-password",
                "/api/health",
                "/swagger",
                "/checkin-hub", // SignalR hub
                "/dashboard-hub"
            };
            
            return !publicPaths.Any(p => path.StartsWith(p));
        }
    }
    
    /// <summary>
    /// Extension method để đăng ký middleware
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
} 