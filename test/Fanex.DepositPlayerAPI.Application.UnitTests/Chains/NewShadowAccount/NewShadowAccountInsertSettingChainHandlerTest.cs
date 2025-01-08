namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewShadowAccount;

using Application.Chains.NewShadowAccount;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewShadowAccountInsertSettingChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly NewShadowAccountInsertSettingChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public NewShadowAccountInsertSettingChainHandlerTest()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _handler = new NewShadowAccountInsertSettingChainHandler(Logger, LogAggregate, _playerRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertShadowAccountSettingSuccess()
    {
        // Arrange
        var request = Fixture.Create<NewShadowAccountChainParam>();
        var logData = Fixture.Create<LogData>();

        _playerRepository
            .InsertShadowAccountSettingAsync(Arg.Any<InsertShadowAccountSettingParam>())
            .Returns(ServiceResult<LogData>.Success(logData));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        request.LogData.Should().Be(logData);
        await _playerRepository.Received(1).InsertShadowAccountSettingAsync(Arg.Is<InsertShadowAccountSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.AgentId == request.AgentId));

        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
        _backgroundTask.Received(1).WriteLogData(_handler.ChainName, request.LogData);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertShadowAccountSettingFailure()
    {
        // Arrange
        var request = Fixture.Create<NewShadowAccountChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository
            .InsertShadowAccountSettingAsync(Arg.Any<InsertShadowAccountSettingParam>())
            .Returns(ServiceResult<LogData>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).InsertShadowAccountSettingAsync(Arg.Is<InsertShadowAccountSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.AgentId == request.AgentId));

        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, request.CustId);
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), Arg.Any<LogData>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertShadowAccountSettingThrowException()
    {
        // Arrange
        var request = Fixture.Create<NewShadowAccountChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository.InsertShadowAccountSettingAsync(Arg.Any<InsertShadowAccountSettingParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).InsertShadowAccountSettingAsync(Arg.Is<InsertShadowAccountSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.AgentId == request.AgentId));

        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, request.CustId);
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), Arg.Any<LogData>());
    }
}