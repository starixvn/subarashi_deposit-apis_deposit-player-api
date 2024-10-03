namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories.Decorate;

using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories.Decorate;
using NSubstitute;

public class CachedServerDbConfigRepositoryTest : CachedRepositoryBaseTest
{
    private readonly IServerDbConfigRepository _decorateRepository;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public CachedServerDbConfigRepositoryTest()
    {
        _serverDbConfigRepository = Substitute.For<IServerDbConfigRepository>();
        _decorateRepository = new CachedServerDbConfigRepository(_serverDbConfigRepository, MemoryCache);
    }

    public override void Dispose()
    {
        _serverDbConfigRepository.ClearReceivedCalls();
        base.Dispose();
    }

    #region GetServerDbConfigByAgentIdAsync

    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_ReturnCachedDataWhenAvailable()
    {
        // Arrange
        var agentId = Fixture.Create<int>();
        var cacheKey = CacheKeys.ServerDbConfigByAgentId(agentId);
        var serverDbConfig = Fixture.Create<ServerDbConfig>();
        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId).Returns(serverDbConfig);
        SetupCacheData(cacheKey, serverDbConfig);

        // Act
        var result = await _decorateRepository.GetServerDbConfigByAgentIdAsync(agentId);

        // Assert
        result.Should().BeEquivalentTo(serverDbConfig);
        await _serverDbConfigRepository.DidNotReceive().GetServerDbConfigByAgentIdAsync(agentId);
    }


    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_FetchDataFromRepositoryWhenCacheIsEmpty()
    {
        // Arrange
        var agentId = Fixture.Create<int>();
        var cacheKey = CacheKeys.ServerDbConfigByAgentId(agentId);
        var serverDbConfig = Fixture.Create<ServerDbConfig>();
        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId).Returns(serverDbConfig);
        SetupNoCacheData(cacheKey);

        // Act
        var result = await _decorateRepository.GetServerDbConfigByAgentIdAsync(agentId);

        // Assert
        result.Should().BeEquivalentTo(serverDbConfig);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(agentId);
    }

    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_ReturnNullWhenDataIsNullFromRepository()
    {
        // Arrange
        var agentId = Fixture.Create<int>();
        var cacheKey = CacheKeys.ServerDbConfigByAgentId(agentId);
        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId).Returns((ServerDbConfig)null!);
        SetupNoCacheData(cacheKey);

        // Act
        var result = await _decorateRepository.GetServerDbConfigByAgentIdAsync(agentId);

        // Assert
        result.Should().BeNull();
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(agentId);
    }

    [Fact]
    public async Task GetServerDbConfigByAgentIdAsync_Should_HandleConcurrentRequestsByReturningCachedDataUntilNewDataIsFetched()
    {
        // Arrange
        var agentId = Fixture.Create<int>();
        var cacheKey = CacheKeys.ServerDbConfigByAgentId(agentId);
        var serverDbConfig = Fixture.Create<ServerDbConfig>();
        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId).Returns(serverDbConfig);
        var count = 0;
        SetupCacheData(cacheKey, serverDbConfig, () => count++ > 0);

        var tasks = new List<Task<ServerDbConfig>>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(_decorateRepository.GetServerDbConfigByAgentIdAsync(agentId));
        }

        // Action
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo(serverDbConfig);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(agentId);
    }

    #endregion

    #region GetAllServerDbConfigAsync

    [Fact]
    public async Task GetAllServerDbConfigAsync_Should_ReturnCachedDataWhenAvailable()
    {
        // Arrange
        var serverDbConfigList = Fixture.CreateMany<ServerDbConfig>().ToList();
        _serverDbConfigRepository.GetAllServerDbConfigAsync().Returns(serverDbConfigList);
        SetupCacheData(CacheKeys.ServerDbConfigAll, serverDbConfigList);

        // Action
        var result = await _decorateRepository.GetAllServerDbConfigAsync();

        // Assert
        result.Should().BeEquivalentTo(serverDbConfigList);
        await _serverDbConfigRepository.DidNotReceive().GetAllServerDbConfigAsync();
    }

    [Fact]
    public async Task GetAllServerDbConfigAsync_Should_FetchDataFromRepositoryWhenCacheIsEmpty()
    {
        // Arrange
        var serverDbConfigList = Fixture.CreateMany<ServerDbConfig>().ToList();
        _serverDbConfigRepository.GetAllServerDbConfigAsync().Returns(serverDbConfigList);
        SetupNoCacheData(CacheKeys.ServerDbConfigAll);

        // Action
        var result = await _decorateRepository.GetAllServerDbConfigAsync();

        // Assert
        result.Should().BeEquivalentTo(serverDbConfigList);
        await _serverDbConfigRepository.Received(1).GetAllServerDbConfigAsync();
    }

    [Fact]
    public async Task GetAllServerDbConfigAsync_Should_ReturnEmptyWhenDataIsNullFromRepository()
    {
        // Arrange
        var emptyServerDbConfigList = new List<ServerDbConfig>();
        _serverDbConfigRepository.GetAllServerDbConfigAsync().Returns(emptyServerDbConfigList);
        SetupNoCacheData(CacheKeys.ServerDbConfigAll);

        // Action
        var result = await _decorateRepository.GetAllServerDbConfigAsync();

        // Assert
        result.Should().BeEmpty();
        await _serverDbConfigRepository.Received(1).GetAllServerDbConfigAsync();
    }

    [Fact]
    public async Task GetAllServerDbConfigAsync_Should_HandleConcurrentRequestsByReturningCachedDataUntilNewDataIsFetched()
    {
        // Arrange
        var serverDbConfigList = Fixture.CreateMany<ServerDbConfig>().ToList();
        _serverDbConfigRepository.GetAllServerDbConfigAsync().Returns(serverDbConfigList);
        var count = 0;
        SetupCacheData(CacheKeys.ServerDbConfigAll, serverDbConfigList, () => count++ > 0);

        var tasks = new List<Task<List<ServerDbConfig>>>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(_decorateRepository.GetAllServerDbConfigAsync());
        }

        // Action
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllBeEquivalentTo(serverDbConfigList);
        await _serverDbConfigRepository.Received(1).GetAllServerDbConfigAsync();
    }

    #endregion
}