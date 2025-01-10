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

public class NewShadowAccountHandlerTest : HandlerTestBase
{
    private readonly IRequestHandler<NewShadowAccountCommand, ServiceResult<ShadowAccount>> _handler;

    public NewShadowAccountHandlerTest()
    {
        _handler = new NewShadowAccountHandler(LogAggregate, ChainHandlerFactory);
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_ReturnSuccess_WhenHandlerReturnSuccess()
    {
        // Arrange
        var command = Fixture.Create<NewShadowAccountCommand>();
        var data = Fixture.Create<ShadowAccount>();
        var expectedResult = ServiceResult<ShadowAccount>.Success(data);
        var newShadowAccountChainHandler = Substitute.For<IChainHandler<NewShadowAccountChainParam, ShadowAccount>>();

        ChainHandlerFactory.CreateNewShadowAccountChainHandler().Returns(newShadowAccountChainHandler);
        newShadowAccountChainHandler.Handle(Arg.Any<NewShadowAccountChainParam>()).Returns(expectedResult);


        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_ReturnFailure_WhenHandlerReturnFailure()
    {
        // Arrange
        var command = Fixture.Create<NewShadowAccountCommand>();
        var expectedResult = ServiceResult<ShadowAccount>.Failure(ServiceErrors.SystemError);
        var newShadowAccountChainHandler = Substitute.For<IChainHandler<NewShadowAccountChainParam, ShadowAccount>>();

        ChainHandlerFactory.CreateNewShadowAccountChainHandler().Returns(newShadowAccountChainHandler);
        newShadowAccountChainHandler.Handle(Arg.Any<NewShadowAccountChainParam>()).Returns(expectedResult);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_ThrowException_WhenChainHandlerIsNull()
    {
        // Arrange
        var command = Fixture.Create<NewShadowAccountCommand>();

        ChainHandlerFactory.CreateNewShadowAccountChainHandler().Returns((IChainHandler<NewShadowAccountChainParam, ShadowAccount>)null!);

        // Action
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_HandleException_WhenRootChainHandlerThrowsException()
    {
        // Arrange
        var chainName = Fixture.Create<string>();
        var command = Fixture.Create<NewShadowAccountCommand>();
        var newShadowAccountChainHandler = Substitute.ForPartsOf<ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>>(Logger, LogAggregate);
        newShadowAccountChainHandler.IsBreakOnError.Returns(true);
        newShadowAccountChainHandler.ChainName.Returns(chainName);
        newShadowAccountChainHandler.HandleInternal(Arg.Any<NewShadowAccountChainParam>()).Throws(new Exception("Test Exception"));

        ChainHandlerFactory.CreateNewShadowAccountChainHandler().Returns(newShadowAccountChainHandler);

        // Action
        var actualResult = await _handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(ServiceResult<Player>.Failure(ServiceErrors.SystemError));
        await Logger.Received(1).ErrorAsync(Arg.Any<string>());
        newShadowAccountChainHandler.Received(1).HandleError(Arg.Any<NewShadowAccountChainParam>());
        LogAggregate.Received(1).AppendLog("Started processing...");
        LogAggregate.Received(1).AppendStepLog($"START PROCESS \"{chainName}\"");
        LogAggregate.Received(1).AppendLog("Finished processing...");
    }
}