namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.ReactivatePlayer;

using Application.Chains.ReactivatePlayer;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class ReactivatePlayerWriteLogDataChainHandlerTest : ChainHandlerTestBase
{
    private readonly ReactivatePlayerWriteLogDataChainHandler _handler;
    private readonly ILogDbRepository _logDbRepository;

    public ReactivatePlayerWriteLogDataChainHandlerTest()
    {
        _logDbRepository = Substitute.For<ILogDbRepository>();
        _handler = new ReactivatePlayerWriteLogDataChainHandler(Logger, LogAggregate, _logDbRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenWriteLogTrackingDBLOGSuccess()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.ExtraInfo.Should().BeNull();
        await _logDbRepository.Received(1).WriteLogTrackingDBLOGAsync(Arg.Is<LogTracking>(x =>
            x.CustId == request.CustId
            && x.Action == "REACTIVATION"
            && x.Status == "OnProgress"
            && x.Message == "Start reactivating customer"));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenWriteLogTrackingDBLOGThrowException()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        _logDbRepository.WriteLogTrackingDBLOGAsync(Arg.Any<LogTracking>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _logDbRepository.Received(1).WriteLogTrackingDBLOGAsync(Arg.Is<LogTracking>(x =>
            x.CustId == request.CustId
            && x.Action == "REACTIVATION"
            && x.Status == "OnProgress"
            && x.Message == "Start reactivating customer"));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x => x.StartsWith($"ERROR: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}