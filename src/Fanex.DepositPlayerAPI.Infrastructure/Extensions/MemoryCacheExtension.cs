namespace Fanex.DepositPlayerAPI.Infrastructure.Extensions;

using Microsoft.Extensions.Caching.Memory;

public static class MemoryCacheExtension
{
    public static async Task<T> GetOrCreateFromCache<T>(this IMemoryCache cache, string key, Func<Task<T>> action, TimeSpan? cacheTime)
        where T : class
    {
        if (!cache.TryGetValue(key, out T data))
        {
            data = await action();
            if (data is not null)
            {
                cache.SetValueToCache(key, data, cacheTime);
            }
        }

        return data;
    }

    public static async Task<T> GetOrCreateFromCacheWithoutExpired<T>(this IMemoryCache cache, string key, Func<Task<T>> action) where T : class
    {
        if (!cache.TryGetValue(key, out T data))
        {
            data = await action();
            if (data is not null)
            {
                cache.SetValueToCacheWithoutExpired(key, data);
            }
        }

        return data;
    }

    public static T GetValueFromCache<T>(this IMemoryCache cache, string key, T defaultValue = default)
    {
        if (!cache.TryGetValue(key, out T data))
        {
            return data;
        }

        return defaultValue;
    }

    public static void RemoveFromCache(this IMemoryCache cache, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key should not be empty!");
        }

        cache.Remove(key);
    }

    public static void SetValueToCache<T>(this IMemoryCache cache, string key, T value, TimeSpan? cacheTime)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key should not be empty!");
        }

        if (!Equals(value, default(T)))
        {
            const int defaultExpiredMinutes = 15;
            cache.Set(key, value, cacheTime ?? TimeSpan.FromMinutes(defaultExpiredMinutes));
        }
    }

    public static void SetValueToCacheWithoutExpired<T>(this IMemoryCache cache, string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key), "Key should not be empty!");
        }

        if (!Equals(value, default(T)))
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove
            };

            cache.Set(key, value, options);
        }
    }
}