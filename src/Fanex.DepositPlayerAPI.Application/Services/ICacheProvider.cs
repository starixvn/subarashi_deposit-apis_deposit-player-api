namespace Fanex.DepositPlayerAPI.Application.Services;

public interface ICacheProvider
{
    public Task<T> GetOrCreateFromCacheAsync<T>(string key, Func<Task<T>> action, TimeSpan? cacheTime);

    public T GetOrCreateFromCache<T>(string key, Func<T> action, TimeSpan? cacheTime);

    public Task<T> GetOrCreateFromCacheWithoutExpiredAsync<T>(string key, Func<Task<T>> action);

    public T GetOrCreateFromCacheWithoutExpired<T>(string key, Func<T> action);

    public T GetValueFromCache<T>(string key, T defaultValue = default);

    public void RemoveFromCache(string key);
}