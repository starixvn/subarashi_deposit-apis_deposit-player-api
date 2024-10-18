namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.ReactivatePlayer;

using Application.Chains.ReactivatePlayer;
using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class ReactivatePlayerGetServerDbConfigChainHandlerTest : ChainHandlerTestBase
{
    private readonly ICacheProvider _cacheProvider;
    private readonly ReactivatePlayerGetServerDbConfigChainHandler _handler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public ReactivatePlayerGetServerDbConfigChainHandlerTest()
    {
        _serverDbConfigRepository = Substitute.For<IServerDbConfigRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _handler = new ReactivatePlayerGetServerDbConfigChainHandler(
            Logger,
            LogAggregate,
            _httpContextAccessor,
            _serverDbConfigRepository,
            _cacheProvider);
    }

    [Fact]
    public async Task ChainHandler_Should_Not_CallGetServerDbConfigByAgentId_WhenHitCache()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var reactivatePlayerChainParam = new ReactivatePlayerChainParam { Recommend = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(new Dictionary<int, ServerDbConfig>
            {
                { dbConfig.CustId, dbConfig }
            });

        // Action
        var result = await _handler.Handle(reactivatePlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        await _serverDbConfigRepository.DidNotReceive().GetServerDbConfigByAgentIdAsync(Arg.Any<int>());
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }

    [Fact]
    public async Task ChainHandler_Should_CallGetServerDbConfigByAgentId_WhenNotHitCache()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var reactivatePlayerChainParam = new ReactivatePlayerChainParam { Recommend = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns(dbConfig);

        // Action
        var result = await _handler.Handle(reactivatePlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenNotFoundServerDbConfigByAgentId()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var reactivatePlayerChainParam = new ReactivatePlayerChainParam { Recommend = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns((ServerDbConfig)null!);

        // Action
        var result = await _handler.Handle(reactivatePlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(PlayerError.CreateCustomerFailed);
        result.ExtraInfo.Should().StartWith($"[ReactivatePlayer] - DB server configuration not found for AgentId={dbConfig.CustId}.");
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        Logger.Received(1).Error(Arg.Is<string>(x => x.StartsWith($"ERROR: An error occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertDidNotReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenGetServerDbConfigThrowException()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var reactivatePlayerChainParam = new ReactivatePlayerChainParam { Recommend = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Throws<Exception>();


        // Action
        var result = await _handler.Handle(reactivatePlayerChainParam);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x => x.StartsWith($"ERROR: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertDidNotReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }
}