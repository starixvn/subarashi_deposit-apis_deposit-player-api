namespace Fanex.DepositPlayerAPI.Application.UnitTests.Handlers;

using Application.Chains;
using Application.Handlers;
using AutoFixture;
using Commands;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Results;
using FluentAssertions;
using Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class ReactivatePlayerHandlerTest
{
    private readonly IChainHandlerFactory _chainHandlerFactory = Substitute.For<IChainHandlerFactory>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogAggregate _logAggregate = Substitute.For<ILogAggregate>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task ReactivatePlayerHandler_Should_ReturnSuccess_WhenHandlerReturnSuccess()
    {
        // Arrange
        var command = _fixture.Create<ReactivatePlayerCommand>();
        var expectedResult = ServiceResult.Success();
        var newPlayerChainHandler = Substitute.For<IChainNonResultHandler<ReactivatePlayerChainParam>>();

        _chainHandlerFactory.CreateReactivatePlayerChainHandler().Returns(newPlayerChainHandler);
        newPlayerChainHandler.Handle(Arg.Any<ReactivatePlayerChainParam>()).Returns(expectedResult);

        var handler = new ReactivatePlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_ReturnFailure_WhenHandlerReturnFailure()
    {
        // Arrange
        var command = _fixture.Create<ReactivatePlayerCommand>();
        var expectedResult = ServiceResult.Failure(ServiceErrors.SystemError);
        var newPlayerChainHandler = Substitute.For<IChainNonResultHandler<ReactivatePlayerChainParam>>();

        _chainHandlerFactory.CreateReactivatePlayerChainHandler().Returns(newPlayerChainHandler);
        newPlayerChainHandler.Handle(Arg.Any<ReactivatePlayerChainParam>()).Returns(expectedResult);

        var handler = new ReactivatePlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_ThrowException_WhenChainHandlerIsNull()
    {
        // Arrange
        var command = _fixture.Create<ReactivatePlayerCommand>();

        _chainHandlerFactory.CreateReactivatePlayerChainHandler().Returns((IChainNonResultHandler<ReactivatePlayerChainParam>)null!);

        var handler = new ReactivatePlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task ReactivatePlayerHandler_Should_HandleException_WhenRootChainHandlerThrowsException()
    {
        // Arrange
        var chainName = _fixture.Create<string>();
        var command = _fixture.Create<ReactivatePlayerCommand>();
        var newPlayerChainHandler = Substitute.ForPartsOf<ChainBaseNonResultHandler<ReactivatePlayerChainParam>>(_logger, _logAggregate);
        newPlayerChainHandler.IsBreakOnError.Returns(true);
        newPlayerChainHandler.ChainName.Returns(chainName);
        newPlayerChainHandler.HandleInternal(Arg.Any<ReactivatePlayerChainParam>()).Throws(new Exception("Test Exception"));

        _chainHandlerFactory.CreateReactivatePlayerChainHandler().Returns(newPlayerChainHandler);

        var handler = new ReactivatePlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(ServiceResult.Failure(ServiceErrors.SystemError));
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        newPlayerChainHandler.Received(1).HandleError(Arg.Any<ReactivatePlayerChainParam>());
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendStepLog($"START PROCESS \"{chainName}\"");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }
}