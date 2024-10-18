namespace Fanex.DepositPlayerAPI.Infrastructure.Services;

using Application.Services;
using Microsoft.Extensions.Caching.Memory;

public class CacheProvider(IMemoryCache cache) : ICacheProvider
{
    public async Task<T> GetOrCreateFromCacheAsync<T>(string key, Func<Task<T>> action, TimeSpan? cacheTime)
    {
        if (!cache.TryGetValue(key, out T data))
        {
            data = await action();
            if (data is not null)
            {
                SetValueToCache(key, data, cacheTime);
            }
        }

        return data;
    }

    public T GetOrCreateFromCache<T>(string key, Func<T> action, TimeSpan? cacheTime)
    {
        if (!cache.TryGetValue(key, out T data))
        {
            data = action();
            if (data is not null)
            {
                SetValueToCache(key, data, cacheTime);
            }
        }

        return data;
    }

    public async Task<T> GetOrCreateFromCacheWithoutExpiredAsync<T>(string key, Func<Task<T>> action)
    {
        if (!cache.TryGetValue(key, out T data))
        {
            data = await action();
            if (data is not null)
            {
                SetValueToCacheWithoutExpired(key, data);
            }
        }

        return data;
    }

    public T GetOrCreateFromCacheWithoutExpired<T>(string key, Func<T> action)
    {
        if (!cache.TryGetValue(key, out T data))
        {
            data = action();
            if (data is not null)
            {
                SetValueToCacheWithoutExpired(key, data);
            }
        }

        return data;
    }

    public T GetValueFromCache<T>(string key, T defaultValue = default)
    {
        return cache.TryGetValue(key, out T data) ? data : defaultValue;
    }

    public void RemoveFromCache(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key should not be empty!");
        }

        cache.Remove(key);
    }

    private void SetValueToCache<T>(string key, T value, TimeSpan? cacheTime)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key should not be empty!");
        }

        if (Equals(value, default(T)))
        {
            return;
        }

        const int defaultExpiredMinutes = 15;
        cache.Set(key, value, cacheTime ?? TimeSpan.FromMinutes(defaultExpiredMinutes));
    }

    private void SetValueToCacheWithoutExpired<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key should not be empty!");
        }

        if (Equals(value, default(T)))
        {
            return;
        }

        var options = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.NeverRemove
        };

        cache.Set(key, value, options);
    }
}