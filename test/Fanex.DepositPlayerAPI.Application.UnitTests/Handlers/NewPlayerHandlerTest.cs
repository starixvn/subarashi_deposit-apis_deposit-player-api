namespace Fanex.DepositPlayerAPI.Application.UnitTests.Handlers;

using Application.Common.Interfaces;
using Application.Handlers;
using AutoFixture;
using Chains;
using Commands;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Results;
using FluentAssertions;
using Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class NewPlayerHandlerTest
{
    private readonly IChainHandlerFactory _chainHandlerFactory = Substitute.For<IChainHandlerFactory>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogAggregate _logAggregate = Substitute.For<ILogAggregate>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task NewPlayerHandler_Should_ReturnSuccess_WhenHandlerReturnSuccess()
    {
        // Arrange
        var command = _fixture.Create<NewPlayerCommand>();
        var data = _fixture.Create<Player>();
        var expectedResult = ServiceResult<Player>.Success(data);
        var newPlayerChainHandler = Substitute.For<IChainHandler<NewPlayerChainParam, Player>>();

        _chainHandlerFactory.CreateNewPlayerChainHandler().Returns(newPlayerChainHandler);
        newPlayerChainHandler.Handle(Arg.Any<NewPlayerChainParam>()).Returns(expectedResult);

        var handler = new NewPlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewPlayerHandler_Should_ReturnFailure_WhenHandlerReturnFailure()
    {
        // Arrange
        var command = _fixture.Create<NewPlayerCommand>();
        var expectedResult = ServiceResult<Player>.Failure(ServiceErrors.SystemError);
        var newPlayerChainHandler = Substitute.For<IChainHandler<NewPlayerChainParam, Player>>();

        _chainHandlerFactory.CreateNewPlayerChainHandler().Returns(newPlayerChainHandler);
        newPlayerChainHandler.Handle(Arg.Any<NewPlayerChainParam>()).Returns(expectedResult);

        var handler = new NewPlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewPlayerHandler_Should_ThrowException_WhenChainHandlerIsNull()
    {
        // Arrange
        var command = _fixture.Create<NewPlayerCommand>();

        _chainHandlerFactory.CreateNewPlayerChainHandler().Returns((IChainHandler<NewPlayerChainParam, Player>)null!);

        var handler = new NewPlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task NewPlayerHandler_Should_HandleException_WhenRootChainHandlerThrowsException()
    {
        // Arrange
        var chainName = _fixture.Create<string>();
        var command = _fixture.Create<NewPlayerCommand>();
        var newPlayerChainHandler = Substitute.ForPartsOf<ChainBaseHandler<NewPlayerChainParam, Player>>(_logger, _logAggregate);
        newPlayerChainHandler.IsBreakOnError.Returns(true);
        newPlayerChainHandler.ChainName.Returns(chainName);
        newPlayerChainHandler.HandleInternal(Arg.Any<NewPlayerChainParam>()).Throws(new Exception("Test Exception"));

        _chainHandlerFactory.CreateNewPlayerChainHandler().Returns(newPlayerChainHandler);

        var handler = new NewPlayerHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(ServiceResult<Player>.Failure(ServiceErrors.SystemError));
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        newPlayerChainHandler.Received(1).HandleError(Arg.Any<NewPlayerChainParam>());
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendStepLog($"START PROCESS \"{chainName}\"");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }
}