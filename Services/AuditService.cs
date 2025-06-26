using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class AuditService : IAuditService
    {
        private readonly IRepository<AuditLog> _auditRepository;
        private readonly ITenantProvider _tenantProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IRepository<AuditLog> auditRepository,
            ITenantProvider tenantProvider,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _auditRepository = auditRepository;
            _tenantProvider = tenantProvider;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync<T>(int entityId, string action, T? oldValue = null, T? newValue = null, string? description = null)
        {
            await LogAsync(typeof(T).Name, entityId, action, oldValue, newValue, description);
        }

        public async Task LogAsync(string entityType, int entityId, string action, object? oldValue = null, object? newValue = null, string? description = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    TenantId = _tenantProvider.GetCurrentTenantId(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                // Get current user ID if available
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    if (int.TryParse(httpContext.User.FindFirst("UserId")?.Value, out int userId))
                    {
                        auditLog.UserId = userId;
                    }
                }

                // Get IP address and user agent
                if (httpContext != null)
                {
                    auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                    auditLog.UserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                }

                // Serialize old and new values
                if (oldValue != null)
                {
                    auditLog.OldValues = JsonConvert.SerializeObject(oldValue, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }

                if (newValue != null)
                {
                    auditLog.NewValues = JsonConvert.SerializeObject(newValue, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                }

                await _auditRepository.AddAsync(auditLog);
                await _auditRepository.SaveChangesAsync();

                _logger.LogInformation("Audit log created: {EntityType} {EntityId} {Action}", entityType, entityId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log for {EntityType} {EntityId} {Action}", entityType, entityId, action);
            }
        }

        public async Task LogUserActionAsync(string action, string? description = null)
        {
            await LogAsync("User", 0, action, null, null, description);
        }

        public async Task LogSystemActionAsync(string action, string? description = null)
        {
            await LogAsync("System", 0, action, null, null, description);
        }
    }
} 