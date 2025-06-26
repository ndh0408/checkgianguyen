using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface IAuditService
    {
        Task LogAsync<T>(int entityId, string action, T? oldValue = null, T? newValue = null, string? description = null);
        Task LogAsync(string entityType, int entityId, string action, object? oldValue = null, object? newValue = null, string? description = null);
        Task LogUserActionAsync(string action, string? description = null);
        Task LogSystemActionAsync(string action, string? description = null);
    }
} 