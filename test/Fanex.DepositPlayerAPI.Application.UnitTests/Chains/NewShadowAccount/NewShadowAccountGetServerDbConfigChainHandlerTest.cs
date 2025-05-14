namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewShadowAccount;

using Application.Chains.NewShadowAccount;
using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewShadowAccountGetServerDbConfigChainHandlerTest : ChainHandlerTestBase
{
    private readonly ICacheProvider _cacheProvider;
    private readonly NewShadowAccountGetServerDbConfigChainHandler _handler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public NewShadowAccountGetServerDbConfigChainHandlerTest()
    {
        _serverDbConfigRepository = Substitute.For<IServerDbConfigRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _handler = new NewShadowAccountGetServerDbConfigChainHandler(
            Logger,
            LogAggregate,
            _serverDbConfigRepository,
            _httpContextAccessor,
            _cacheProvider);
    }

    [Fact]
    public async Task ChainHandler_Should_Not_CallGetServerDbConfigByAgentId_WhenHitCache()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var newShadowAccountChainParam = new NewShadowAccountChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(new Dictionary<int, ServerDbConfig>
            {
                { dbConfig.CustId, dbConfig }
            });

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
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
        var newShadowAccountChainParam = new NewShadowAccountChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns(dbConfig);

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
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
        var newShadowAccountChainParam = new NewShadowAccountChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns((ServerDbConfig)null!);

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(PlayerError.CreateCustomerFailed);
        result.ExtraInfo.Should().StartWith($"[{_handler.FunctionName}] - DB server configuration not found for AgentId={dbConfig.CustId}.");
        result.Result.Should().BeNull();
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(PlayerError.CreateCustomerFailed)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertDidNotReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenGetServerDbConfigThrowException()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var newShadowAccountChainParam = new NewShadowAccountChainParam { AgentId = dbConfig.CustId };
        _cacheProvider
            .GetValueFromCache(CacheKeys.ServerDbConfigAll, Arg.Any<Dictionary<int, ServerDbConfig>>())
            .Returns(callInfo => callInfo[1]);

        _cacheProvider
            .GetOrCreateFromCacheWithoutExpiredAsync(CacheKeys.ServerDbConfigByAgentId(dbConfig.CustId), Arg.Any<Func<Task<ServerDbConfig>>>())
            .Returns(async callInfo => await ((Func<Task<ServerDbConfig>>)callInfo[1])());

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Throws<Exception>();


        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceError.SystemError : ServiceError.None);
        result.Result.Should().BeNull();
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertDidNotReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }
}