namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories.Decorate;

using AutoFixture;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NSubstitute.ClearExtensions;

public abstract class CachedRepositoryBaseTest : IDisposable
{
    protected readonly IFixture Fixture = new Fixture();
    protected readonly IMemoryCache MemoryCache = Substitute.For<IMemoryCache>();

    public virtual void Dispose()
    {
        MemoryCache.ClearSubstitute();
    }

    protected void SetupCacheData(string key, object data)
    {
        MemoryCache
            .TryGetValue(Arg.Is<object>(k => k.ToString() == key), out _)
            .Returns(x =>
            {
                x[1] = data;
                return true;
            });
    }

    protected void SetupNoCacheData(string key)
    {
        MemoryCache
            .TryGetValue(Arg.Is<object>(k => k.ToString() == key), out _)
            .Returns(false);
    }
}