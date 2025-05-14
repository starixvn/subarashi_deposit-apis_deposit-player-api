namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Services;

using Application.Services;
using AutoFixture;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using NSubstitute.ClearExtensions;

public class CacheProviderTest : IDisposable
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IFixture _fixture;
    private readonly IMemoryCache _memoryCache;

    public CacheProviderTest()
    {
        _fixture = new Fixture();
        _memoryCache = Substitute.For<IMemoryCache>();
        _cacheProvider = new CacheProvider(_memoryCache);
    }

    public virtual void Dispose()
    {
        _memoryCache.ClearSubstitute();
    }

    [Fact]
    public async Task GetOrCreateFromCacheAsync_Should_ReturnCachedData_WhenHitCache()
    {
        // Arrange
        var cacheData = _fixture.Create<string>();
        var key = _fixture.Create<string>();

        SetupCacheData(key, cacheData);

        // Action
        var actualData = await _cacheProvider.GetOrCreateFromCacheAsync(
            key,
            () => Task.FromResult<string>(null!),
            TimeSpan.FromMinutes(5));

        // Assert
        actualData.Should().Be(cacheData);
        _memoryCache.DidNotReceiveWithAnyArgs().CreateEntry(key);
    }

    [Fact]
    public async Task GetOrCreateFromCacheAsync_Should_ReturnFetchData_WhenNotHitCache()
    {
        // Arrange
        var data = _fixture.Create<string>();
        var key = _fixture.Create<string>();
        var entrySub = Substitute.For<ICacheEntry>();
        _memoryCache.CreateEntry(key).Returns(entrySub);
        SetupNoCacheData(key);

        // Action
        var actualData = await _cacheProvider.GetOrCreateFromCacheAsync(
            key,
            () => Task.FromResult(data),
            TimeSpan.FromMinutes(5));

        // Assert
        actualData.Should().Be(data);
        _memoryCache.Received(1).CreateEntry(key);
        entrySub.Received(1).AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    }

    [Fact]
    public void GetOrCreateFromCache_Should_ReturnCachedData_WhenHitCache()
    {
        // Arrange
        var cacheData = _fixture.Create<string>();
        var key = _fixture.Create<string>();

        SetupCacheData(key, cacheData);

        // Action
        var actualData = _cacheProvider.GetOrCreateFromCache(
            key,
            string () => null!,
            TimeSpan.FromMinutes(5));

        // Assert
        actualData.Should().Be(cacheData);
        _memoryCache.DidNotReceiveWithAnyArgs().CreateEntry(key);
    }

    [Fact]
    public void GetOrCreateFromCache_Should_ReturnFetchData_WhenNotHitCache()
    {
        // Arrange
        var data = _fixture.Create<string>();
        var key = _fixture.Create<string>();
        var entrySub = Substitute.For<ICacheEntry>();
        _memoryCache.CreateEntry(key).Returns(entrySub);
        SetupNoCacheData(key);

        // Action
        var actualData = _cacheProvider.GetOrCreateFromCache(
            key,
            () => data,
            TimeSpan.FromMinutes(5));

        // Assert
        actualData.Should().Be(data);
        _memoryCache.Received(1).CreateEntry(key);
        entrySub.Received(1).AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    }

    [Fact]
    public async Task GetOrCreateFromCacheWithoutExpiredAsync_Should_ReturnCacheData_WhenHitCache()
    {
        // Arrange
        var cacheData = _fixture.Create<string>();
        var key = _fixture.Create<string>();
        SetupCacheData(key, cacheData);

        // Action
        var actualData = await _cacheProvider.GetOrCreateFromCacheWithoutExpiredAsync(
            key,
            () => Task.FromResult<string>(null!));

        // Assert
        actualData.Should().Be(cacheData);
        _memoryCache.DidNotReceiveWithAnyArgs().CreateEntry(key);
    }

    [Fact]
    public async Task GetOrCreateFromCacheWithoutExpiredAsync_Should_ReturnFetchData_WhenNotHitCache()
    {
        // Arrange
        var data = _fixture.Create<string>();
        var key = _fixture.Create<string>();
        var entrySub = Substitute.For<ICacheEntry>();
        _memoryCache.CreateEntry(key).Returns(entrySub);
        SetupNoCacheData(key);

        // Action
        var actualData = await _cacheProvider.GetOrCreateFromCacheWithoutExpiredAsync(
            key,
            () => Task.FromResult(data));

        // Assert
        actualData.Should().Be(data);
        _memoryCache.Received(1).CreateEntry(key);
    }

    [Fact]
    public void RemoveFromCache_Should_RemoveCache()
    {
        // Arrange
        var key = _fixture.Create<string>();

        // Action
        _cacheProvider.RemoveFromCache(key);

        // Assert
        _memoryCache.Received(1).Remove(key);
    }

    [Fact]
    public void GetValueFromCache_Should_ReturnCacheData_WhenHitCache()
    {
        // Arrange
        var data = _fixture.Create<string>();
        var key = _fixture.Create<string>();
        SetupCacheData(key, data);

        // Action
        var actualData = _cacheProvider.GetValueFromCache(key, string.Empty);

        // Assert
        actualData.Should().Be(data);
        _memoryCache.DidNotReceive().CreateEntry(key);
    }


    [Fact]
    public void GetValueFromCache_Should_ReturnDefaultData_WhenNotHitCache()
    {
        // Arrange
        var defaultData = _fixture.Create<string>();
        var key = _fixture.Create<string>();
        SetupNoCacheData(key);

        // Action
        var actualData = _cacheProvider.GetValueFromCache(key, defaultData);

        // Assert
        actualData.Should().Be(defaultData);
        _memoryCache.DidNotReceive().CreateEntry(key);
    }

    private void SetupCacheData(string key, object data, Func<bool> func = null)
    {
        _memoryCache
            .TryGetValue(Arg.Is<object>(k => k.ToString() == key), out _)
            .Returns(x =>
            {
                x[1] = data;
                return func is null || func();
            });
    }

    private void SetupNoCacheData(string key)
    {
        _memoryCache
            .TryGetValue(Arg.Is<object>(k => k.ToString() == key), out _)
            .Returns(false);
    }
}