namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.ReactivatePlayer;

using Application.Chains.ReactivatePlayer;
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

public class ReactivatePlayerGetArchivedInfoChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly ReactivatePlayerGetArchivedInfoChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public ReactivatePlayerGetArchivedInfoChainHandlerTest()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _handler = new ReactivatePlayerGetArchivedInfoChainHandler(Logger, LogAggregate, _playerRepository, _backgroundTask);
    }


    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenGetArchivedInfoSuccess()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var outputParam = Fixture.Create<GetArchivedPlayerInfoOutputParam>();

        _playerRepository.GetArchivedInfoAsync(Arg.Any<int>()).Returns(ServiceResult<GetArchivedPlayerInfoOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        request.CustomerInfo.Should().Be(outputParam.CustomerInfo);
        request.LogTracking.Should().Be(outputParam.LogTracking);
        request.Recommend.Should().Be(outputParam.Recommend);
        await _playerRepository.Received(1).GetArchivedInfoAsync(request.CustId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenGetArchivedInfoFailureWithLogTracking()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var outputParam = Fixture.Create<GetArchivedPlayerInfoOutputParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository
            .GetArchivedInfoAsync(Arg.Any<int>())
            .Returns(ServiceResult<GetArchivedPlayerInfoOutputParam>.FailureWithData(ServiceErrors.SystemError, outputParam, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _playerRepository.Received(1).GetArchivedInfoAsync(request.CustId);
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenGetArchivedInfoFailureWithoutLogTracking()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var outputParam = Fixture
            .Build<GetArchivedPlayerInfoOutputParam>()
            .Without(x => x.LogTracking)
            .Create();
        var extraInfo = Fixture.Create<string>();

        _playerRepository
            .GetArchivedInfoAsync(Arg.Any<int>())
            .Returns(ServiceResult<GetArchivedPlayerInfoOutputParam>.FailureWithData(ServiceErrors.SystemError, outputParam, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _playerRepository.Received(1).GetArchivedInfoAsync(request.CustId);
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenGetArchivedInfoThrowException()
    {
        // Arrange
        var request = Fixture
            .Build<ReactivatePlayerChainParam>()
            .Without(x => x.LogTracking)
            .Create();

        _playerRepository.GetArchivedInfoAsync(Arg.Any<int>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _playerRepository.Received(1).GetArchivedInfoAsync(request.CustId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
    }
}