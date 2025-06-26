using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            return await Task.FromResult(_memoryCache.Get<T>(key));
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new MemoryCacheEntryOptions();
            if (expiry.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiry;
            }

            _memoryCache.Set(key, value, options);
            await Task.CompletedTask;
        }

        public async Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            await Task.CompletedTask;
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            // Note: MemoryCache doesn't support pattern removal natively
            // In production, consider using Redis or implement a custom solution
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            await SetAsync(key, value, expiry);
            return value;
        }
    }
} 