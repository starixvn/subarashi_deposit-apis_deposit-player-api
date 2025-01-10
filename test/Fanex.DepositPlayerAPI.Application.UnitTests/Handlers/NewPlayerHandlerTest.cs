namespace Fanex.DepositPlayerAPI.Application.UnitTests.Handlers;

using Application.Chains;
using Application.Handlers;
using AutoFixture;
using Commands;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Results;
using FluentAssertions;
using MediatR;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class NewPlayerHandlerTest : HandlerTestBase
{
    private readonly IRequestHandler<NewPlayerCommand, ServiceResult<Player>> _handler;

    public NewPlayerHandlerTest()
    {
        _handler = new NewPlayerHandler(LogAggregate, ChainHandlerFactory);
    }

    [Fact]
    public async Task NewPlayerHandler_Should_ReturnSuccess_WhenHandlerReturnSuccess()
    {
        // Arrange
        var command = Fixture.Create<NewPlayerCommand>();
        var data = Fixture.Create<Player>();
        var expectedResult = ServiceResult<Player>.Success(data);
        var newPlayerChainHandler = Substitute.For<IChainHandler<NewPlayerChainParam, Player>>();

        ChainHandlerFactory.CreateNewPlayerChainHandler().Returns(newPlayerChainHandler);
        newPlayerChainHandler.Handle(Arg.Any<NewPlayerChainParam>()).Returns(expectedResult);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewPlayerHandler_Should_ReturnFailure_WhenHandlerReturnFailure()
    {
        // Arrange
        var command = Fixture.Create<NewPlayerCommand>();
        var expectedResult = ServiceResult<Player>.Failure(ServiceError.SystemError);
        var newPlayerChainHandler = Substitute.For<IChainHandler<NewPlayerChainParam, Player>>();

        ChainHandlerFactory.CreateNewPlayerChainHandler().Returns(newPlayerChainHandler);
        newPlayerChainHandler.Handle(Arg.Any<NewPlayerChainParam>()).Returns(expectedResult);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewPlayerHandler_Should_ThrowException_WhenChainHandlerIsNull()
    {
        // Arrange
        var command = Fixture.Create<NewPlayerCommand>();

        ChainHandlerFactory.CreateNewPlayerChainHandler().Returns((IChainHandler<NewPlayerChainParam, Player>)null!);

        // Action
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task NewPlayerHandler_Should_HandleException_WhenRootChainHandlerThrowsException()
    {
        // Arrange
        var chainName = Fixture.Create<string>();
        var command = Fixture.Create<NewPlayerCommand>();
        var newPlayerChainHandler = Substitute.ForPartsOf<ChainBaseHandler<NewPlayerChainParam, Player>>(Logger, LogAggregate);
        newPlayerChainHandler.IsBreakOnError.Returns(true);
        newPlayerChainHandler.ChainName.Returns(chainName);
        newPlayerChainHandler.HandleInternal(Arg.Any<NewPlayerChainParam>()).Throws(new Exception("Test Exception"));

        ChainHandlerFactory.CreateNewPlayerChainHandler().Returns(newPlayerChainHandler);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(ServiceResult<Player>.Failure(ServiceError.SystemError));
        await Logger.Received(1).ErrorAsync(Arg.Any<string>());
        newPlayerChainHandler.Received(1).HandleError(Arg.Any<NewPlayerChainParam>());
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendStepLog($"START PROCESS \"{chainName}\"");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }
}