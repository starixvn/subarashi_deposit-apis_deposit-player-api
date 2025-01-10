namespace Fanex.DepositPlayerAPI.Application.UnitTests.Handlers;

using Application.Chains;
using Application.Handlers;
using AutoFixture;
using Commands;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Results;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class ReactivatePlayerHandlerTest : HandlerTestBase
{
    private readonly IRequestHandler<ReactivatePlayerCommand, ServiceResult> _handler;

    public ReactivatePlayerHandlerTest()
    {
        _handler = new ReactivatePlayerHandler(LogAggregate, ChainHandlerFactory);
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_ReturnSuccess_WhenHandlerReturnSuccess()
    {
        // Arrange
        var command = Fixture.Create<ReactivatePlayerCommand>();
        var expectedResult = ServiceResult.Success();
        var reactivatePlayerChainParam = Substitute.For<IChainNonResultHandler<ReactivatePlayerChainParam>>();

        ChainHandlerFactory.CreateReactivatePlayerChainHandler().Returns(reactivatePlayerChainParam);
        reactivatePlayerChainParam.Handle(Arg.Any<ReactivatePlayerChainParam>()).Returns(expectedResult);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_ReturnFailure_WhenHandlerReturnFailure()
    {
        // Arrange
        var command = Fixture.Create<ReactivatePlayerCommand>();
        var expectedResult = ServiceResult.Failure(ServiceErrors.SystemError);
        var reactivatePlayerChainParam = Substitute.For<IChainNonResultHandler<ReactivatePlayerChainParam>>();

        ChainHandlerFactory.CreateReactivatePlayerChainHandler().Returns(reactivatePlayerChainParam);
        reactivatePlayerChainParam.Handle(Arg.Any<ReactivatePlayerChainParam>()).Returns(expectedResult);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_ThrowException_WhenChainHandlerIsNull()
    {
        // Arrange
        var command = Fixture.Create<ReactivatePlayerCommand>();

        ChainHandlerFactory.CreateReactivatePlayerChainHandler().Returns((IChainNonResultHandler<ReactivatePlayerChainParam>)null!);

        // Action
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_HandleException_WhenRootChainHandlerThrowsException()
    {
        // Arrange
        var chainName = Fixture.Create<string>();
        var command = Fixture.Create<ReactivatePlayerCommand>();
        var reactivatePlayerHandler = Substitute.ForPartsOf<ChainBaseNonResultHandler<ReactivatePlayerChainParam>>(Logger, LogAggregate);
        reactivatePlayerHandler.IsBreakOnError.Returns(true);
        reactivatePlayerHandler.ChainName.Returns(chainName);
        reactivatePlayerHandler.HandleInternal(Arg.Any<ReactivatePlayerChainParam>()).Throws(new Exception("Test Exception"));

        ChainHandlerFactory.CreateReactivatePlayerChainHandler().Returns(reactivatePlayerHandler);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(ServiceResult.Failure(ServiceErrors.SystemError));
        await Logger.Received(1).ErrorAsync(Arg.Any<string>());
        reactivatePlayerHandler.Received(1).HandleError(Arg.Any<ReactivatePlayerChainParam>());
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendStepLog($"START PROCESS \"{chainName}\"");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }
}