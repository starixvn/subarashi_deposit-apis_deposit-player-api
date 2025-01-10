namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.MapSabaId;

using Application.Chains.MapSabaId;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class MapSabaIdGetServerDbConfigChainHandlerTest : ChainHandlerTestBase
{
    private readonly MapSabaIdGetServerDbConfigChainHandler _handler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public MapSabaIdGetServerDbConfigChainHandlerTest()
    {
        _serverDbConfigRepository = Substitute.For<IServerDbConfigRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _handler = new MapSabaIdGetServerDbConfigChainHandler(
            Logger,
            LogAggregate,
            _httpContextAccessor,
            _serverDbConfigRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnCorrectData()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var mapSabaIdChainParam = new MapSabaIdChainParam { CustId = dbConfig.CustId };

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns(dbConfig);

        // Action
        var result = await _handler.Handle(mapSabaIdChainParam);

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
    public async Task ChainHandler_Should_ReturnFailure_WhenNotFoundServerDBConfigByCustId()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var mapSabaIdChainParam = new MapSabaIdChainParam { CustId = dbConfig.CustId };

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Returns((ServerDbConfig)null!);

        // Action
        var result = await _handler.Handle(mapSabaIdChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().StartWith($"[{_handler.FunctionName}] - DB server configuration not found for CustId={dbConfig.CustId}.");
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertDidNotReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenGetServerDbConfigThrowException()
    {
        // Arrange
        var dbConfig = Fixture.Create<ServerDbConfig>();
        var mapSabaIdChainParam = new MapSabaIdChainParam { CustId = dbConfig.CustId };

        _serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(dbConfig.CustId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(mapSabaIdChainParam);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _serverDbConfigRepository.Received(1).GetServerDbConfigByAgentIdAsync(dbConfig.CustId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        AssertDidNotReceivedHttpContextAccessor(dbConfig, _httpContextAccessor);
    }
}