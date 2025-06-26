using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public interface IDistributedCacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
        Task<T> GetOrSetHybridAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
    }
} 