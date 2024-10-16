namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Services;

using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NSubstitute.ClearExtensions;

public class CacheProviderTest : IDisposable
{
    protected readonly IFixture Fixture = new Fixture();
    protected readonly IMemoryCache MemoryCache = Substitute.For<IMemoryCache>();

    public virtual void Dispose()
    {
        MemoryCache.ClearSubstitute();
    }

    private void SetupCacheData(string key, object data, Func<bool>? func = null)
    {
        MemoryCache
            .TryGetValue(Arg.Is<object>(k => k.ToString() == key), out _)
            .Returns(x =>
            {
                x[1] = data;
                return func is null || func();
            });
    }

    private void SetupNoCacheData(string key)
    {
        MemoryCache
            .TryGetValue(Arg.Is<object>(k => k.ToString() == key), out _)
            .Returns(false);
    }
}