using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace JokerNutrition.Business.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, bool> _keys = new();

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            if (cachedValue is not null)
            {
                return cachedValue;
            }
        }

        T value = await factory();

        var cacheEntryOptions = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
        }

        cacheEntryOptions.RegisterPostEvictionCallback((evictedKey, val, reason, state) =>
        {
            _keys.TryRemove(evictedKey.ToString() ?? "", out _);
        });

        _memoryCache.Set(key, value, cacheEntryOptions);
        _keys.TryAdd(key, true);

        return value;
    }

    public void Evict(string key)
    {
        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
    }

    public void EvictByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }
}
