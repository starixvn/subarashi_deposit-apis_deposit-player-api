namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using FluentAssertions;
using Logging;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewPlayerGetServerDbConfigChainHandlerTest
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IFixture _fixture;
    private readonly NewPlayerGetServerDbConfigChainHandler _handler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogAggregate _logAggregate;
    private readonly ILogger _logger;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public NewPlayerGetServerDbConfigChainHandlerTest()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger>();
        _logAggregate = Substitute.For<ILogAggregate>();
        _serverDbConfigRepository = Substitute.For<IServerDbConfigRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _handler = new NewPlayerGetServerDbConfigChainHandler(
            _logger,
            _logAggregate,
            _serverDbConfigRepository,
            _httpContextAccessor,
            _cacheProvider);
    }

    [Fact]
    public async Task ChainHandler_Should_Not_CallGetServerDbConfigByAgentId_WhenHitCache()
    {
        // Arrange
        var dbConfig = _fixture.Create<ServerDbConfig>();
        var newPlayerChainParam = new NewPlayerChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(new Dictionary<int, ServerDbConfig>
            {
                { dbConfig.CustId, dbConfig }
            });

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        await _serverDbConfigRepository.DidNotReceive().GetServerDbConfigByAgentIdAsync(Arg.Any<int>());
        AssertReceivedHttpContextAccessor(dbConfig);
    }

    [Fact]
    public async Task ChainHandler_Should_CallGetServerDbConfigByAgentId_WhenNotHitCache()
    {
        // Arrange
        var dbConfig = _fixture.Create<ServerDbConfig>();
        var newPlayerChainParam = new NewPlayerChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns(dbConfig);

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        AssertReceivedHttpContextAccessor(dbConfig);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenNotFoundServerDbConfigByAgentId()
    {
        // Arrange
        var dbConfig = _fixture.Create<ServerDbConfig>();
        var newPlayerChainParam = new NewPlayerChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns((ServerDbConfig)null!);

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(PlayerError.CreateCustomerFailed);
        result.ExtraInfo.Should().StartWith($"[NewPlayer] - DB server configuration not found for AgentId={dbConfig.CustId}.");
        result.Result.Should().BeNull();
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        AssertDidNotReceivedHttpContextAccessor(dbConfig);
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenGetServerDbConfigThrowException()
    {
        // Arrange
        var dbConfig = _fixture.Create<ServerDbConfig>();
        var newPlayerChainParam = new NewPlayerChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Throws<Exception>();


        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(ServiceErrors.SystemError);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        AssertDidNotReceivedHttpContextAccessor(dbConfig);
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    private void AssertReceivedHttpContextAccessor(ServerDbConfig config)
    {
        _httpContextAccessor.HttpContext.Items.Received(1)["CustDbGroupIdKey"] = config.CustDbGroupId;
        _httpContextAccessor.HttpContext.Items.Received(1)["SiteDbGroupIdKey"] = config.SiteDbGroupId;
        _httpContextAccessor.HttpContext.Items.Received(1)["CustDbServerKey"] = config.CustDbServer;
        _httpContextAccessor.HttpContext.Items.Received(1)["SiteDbServerKey"] = config.SiteDbServer;
        _httpContextAccessor.HttpContext.Items.Received(1)["SiteKey"] = config.Site;
    }

    private void AssertDidNotReceivedHttpContextAccessor(ServerDbConfig config)
    {
        _httpContextAccessor.HttpContext.Items.DidNotReceive()["CustDbGroupIdKey"] = config.CustDbGroupId;
        _httpContextAccessor.HttpContext.Items.DidNotReceive()["SiteDbGroupIdKey"] = config.SiteDbGroupId;
        _httpContextAccessor.HttpContext.Items.DidNotReceive()["CustDbServerKey"] = config.CustDbServer;
        _httpContextAccessor.HttpContext.Items.DidNotReceive()["SiteDbServerKey"] = config.SiteDbServer;
        _httpContextAccessor.HttpContext.Items.DidNotReceive()["SiteKey"] = config.Site;
    }
}