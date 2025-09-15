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
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class ReactivateBalanceInitializeChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly ReactivateBalanceInitializeChainHandler _handler;

    public ReactivateBalanceInitializeChainHandlerTest()
    {
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _handler = new ReactivateBalanceInitializeChainHandler(Logger, LogAggregate, _balanceRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeReactivatePlayerBalanceSuccess()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var logTracking = Fixture.Create<LogTracking>();

        _balanceRepository
            .InitializeReactivatePlayerBalanceAsync(Arg.Any<InitializeReactivatePlayerBalanceParam>())
            .Returns(ServiceResult<LogTracking>.Success(logTracking));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        request.LogTracking.Should().Be(logTracking);

        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(Arg.Is<InitializeReactivatePlayerBalanceParam>(x =>
            x.CustId == request.CustId
            && x.CustIdBalanceDummy == request.CustIdBalanceDummy
            && x.ClosingBalance == request.ClosingBalance));

        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeReactivatePlayerBalanceSuccessWithNullResult()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();

        _balanceRepository
            .InitializeReactivatePlayerBalanceAsync(Arg.Any<InitializeReactivatePlayerBalanceParam>())
            .Returns(ServiceResult<LogTracking>.Success(null));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);

        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(Arg.Is<InitializeReactivatePlayerBalanceParam>(x =>
            x.CustId == request.CustId
            && x.CustIdBalanceDummy == request.CustIdBalanceDummy
            && x.ClosingBalance == request.ClosingBalance));

        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }



    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInitializeReactivatePlayerBalanceThrowException()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();

        _balanceRepository
            .InitializeReactivatePlayerBalanceAsync(Arg.Any<InitializeReactivatePlayerBalanceParam>())
            .Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceError.SystemError : ServiceError.None);

        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(Arg.Is<InitializeReactivatePlayerBalanceParam>(x =>
            x.CustId == request.CustId
            && x.CustIdBalanceDummy == request.CustIdBalanceDummy
            && x.ClosingBalance == request.ClosingBalance));

        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
    }

    [Fact]
    public void ChainHandler_Should_HaveCorrectProperties()
    {
        // Assert
        _handler.IsBreakOnError.Should().BeFalse();
        _handler.IsBreakOnSuccess.Should().BeTrue();
        _handler.ChainName.Should().Be(nameof(ReactivateBalanceInitializeChainHandler));
        _handler.FunctionName.Should().Be("ReactivatePlayer");
    }

    [Fact]
    public void HandleError_Should_CallWriteLogTracking_WhenLogTrackingIsNotNull()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();

        // Action
        _handler.HandleError(request);

        // Assert
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
        LogAggregate.Received(1).AppendLog($"Background Task: {nameof(_backgroundTask.WriteLogTracking)}");
    }

    [Fact]
    public void HandleError_Should_NotCallWriteLogTracking_WhenLogTrackingIsNull()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        request.LogTracking = null;

        // Action
        _handler.HandleError(request);

        // Assert
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
        LogAggregate.DidNotReceive().AppendLog(Arg.Any<string>());
    }

    [Fact]
    public void HandleError_Should_NotCallWriteLogTracking_WhenRequestIsNull()
    {
        // Action
        _handler.HandleError(null);

        // Assert
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
        LogAggregate.DidNotReceive().AppendLog(Arg.Any<string>());
    }

    [Fact]
    public void HandleSuccess_Should_NotCallWriteLogTracking_WhenLogTrackingIsNull()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        request.LogTracking = null;

        // Action
        _handler.HandleSuccess(request);

        // Assert
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
        LogAggregate.DidNotReceive().AppendLog(Arg.Any<string>());
    }

    [Fact]
    public void HandleSuccess_Should_NotCallWriteLogTracking_WhenRequestIsNull()
    {
        // Action
        _handler.HandleSuccess(null);

        // Assert
        _backgroundTask.DidNotReceive().WriteLogTracking(Arg.Any<string>(), Arg.Any<LogTracking>());
        LogAggregate.DidNotReceive().AppendLog(Arg.Any<string>());
    }
}
