using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace GiaNguyenCheck.Services
{
    public class DistributedCacheService : IDistributedCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCacheService> _logger;

        public DistributedCacheService(
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ILogger<DistributedCacheService> logger)
        {
            _memoryCache = memoryCache;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                // Check memory cache first (L1)
                if (_memoryCache.TryGetValue(key, out T? memoryValue))
                {
                    _logger.LogDebug("Cache hit in memory for key: {Key}", key);
                    return memoryValue;
                }

                // Check distributed cache (L2)
                var distributedValue = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    var value = JsonConvert.DeserializeObject<T>(distributedValue);
                    if (value != null)
                    {
                        // Cache in memory for faster subsequent access
                        _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                        _logger.LogDebug("Cache hit in distributed cache for key: {Key}", key);
                        return value;
                    }
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiry;
                }

                var jsonValue = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, jsonValue, options);

                // Also set in memory cache for faster access
                var memoryExpiry = expiry ?? TimeSpan.FromMinutes(5);
                _memoryCache.Set(key, value, memoryExpiry);

                _logger.LogDebug("Value cached successfully for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _memoryCache.Remove(key);
                _logger.LogDebug("Value removed from cache for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                // Note: Redis supports pattern removal, but IDistributedCache doesn't
                // In production, consider using StackExchange.Redis directly for pattern removal
                _logger.LogWarning("Pattern removal not supported in current implementation for pattern: {Pattern}", pattern);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing values by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                // Check memory cache first
                if (_memoryCache.TryGetValue(key, out _))
                {
                    return true;
                }

                // Check distributed cache
                var value = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var value = await factory();
            await SetAsync(key, value, expiry);
            return value;
        }

        public async Task<T> GetOrSetHybridAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            try
            {
                // Check memory cache first (L1)
                if (_memoryCache.TryGetValue(key, out T? memoryValue))
                {
                    _logger.LogDebug("Hybrid cache hit in memory for key: {Key}", key);
                    return memoryValue;
                }

                // Check distributed cache (L2)
                var distributedValue = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    var value = JsonConvert.DeserializeObject<T>(distributedValue);
                    if (value != null)
                    {
                        // Cache in memory for faster subsequent access
                        _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                        _logger.LogDebug("Hybrid cache hit in distributed cache for key: {Key}", key);
                        return value;
                    }
                }

                // Get from source and cache in both layers
                var newValue = await factory();
                await SetBothCaches(key, newValue, expiry);
                _logger.LogDebug("Hybrid cache miss, value retrieved from source for key: {Key}", key);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in hybrid cache operation for key: {Key}", key);
                // Fallback to source
                return await factory();
            }
        }

        private async Task SetBothCaches<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                // Set in distributed cache
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiry;
                }

                var jsonValue = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, jsonValue, options);

                // Set in memory cache with shorter expiry
                var memoryExpiry = expiry ?? TimeSpan.FromMinutes(5);
                _memoryCache.Set(key, value, memoryExpiry);

                _logger.LogDebug("Value set in both cache layers for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value in both cache layers for key: {Key}", key);
            }
        }
    }
} 