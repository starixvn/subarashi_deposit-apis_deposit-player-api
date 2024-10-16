namespace Fanex.DepositPlayerAPI.Application.UnitTests.Handlers;

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
using Services;

public class NewShadowAccountHandlerTest
{
    private readonly IChainHandlerFactory _chainHandlerFactory = Substitute.For<IChainHandlerFactory>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogAggregate _logAggregate = Substitute.For<ILogAggregate>();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task NewShadowAccountHandler_Should_ReturnSuccess_WhenHandlerReturnSuccess()
    {
        // Arrange
        var command = _fixture.Create<NewShadowAccountCommand>();
        var data = _fixture.Create<ShadowAccount>();
        var expectedResult = ServiceResult<ShadowAccount>.Success(data);
        var newShadowAccountChainHandler = Substitute.For<IChainHandler<NewShadowAccountChainParam, ShadowAccount>>();

        _chainHandlerFactory.CreateNewShadowAccountChainHandler().Returns(newShadowAccountChainHandler);
        newShadowAccountChainHandler.Handle(Arg.Any<NewShadowAccountChainParam>()).Returns(expectedResult);

        var handler = new NewShadowAccountHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_ReturnFailure_WhenHandlerReturnFailure()
    {
        // Arrange
        var command = _fixture.Create<NewShadowAccountCommand>();
        var expectedResult = ServiceResult<ShadowAccount>.Failure(ServiceErrors.SystemError);
        var newShadowAccountChainHandler = Substitute.For<IChainHandler<NewShadowAccountChainParam, ShadowAccount>>();

        _chainHandlerFactory.CreateNewShadowAccountChainHandler().Returns(newShadowAccountChainHandler);
        newShadowAccountChainHandler.Handle(Arg.Any<NewShadowAccountChainParam>()).Returns(expectedResult);

        var handler = new NewShadowAccountHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(expectedResult);
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_ThrowException_WhenChainHandlerIsNull()
    {
        // Arrange
        var command = _fixture.Create<NewShadowAccountCommand>();

        _chainHandlerFactory.CreateNewShadowAccountChainHandler().Returns((IChainHandler<NewShadowAccountChainParam, ShadowAccount>)null!);

        var handler = new NewShadowAccountHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task NewShadowAccountHandler_Should_HandleException_WhenRootChainHandlerThrowsException()
    {
        // Arrange
        var chainName = _fixture.Create<string>();
        var command = _fixture.Create<NewShadowAccountCommand>();
        var newShadowAccountChainHandler = Substitute.ForPartsOf<ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>>(_logger, _logAggregate);
        newShadowAccountChainHandler.IsBreakOnError.Returns(true);
        newShadowAccountChainHandler.ChainName.Returns(chainName);
        newShadowAccountChainHandler.HandleInternal(Arg.Any<NewShadowAccountChainParam>()).Throws(new Exception("Test Exception"));

        _chainHandlerFactory.CreateNewShadowAccountChainHandler().Returns(newShadowAccountChainHandler);

        var handler = new NewShadowAccountHandler(_logAggregate, _chainHandlerFactory);

        // Action
        var actualResult = await handler.Handle(command, CancellationToken.None);

        // Assert
        actualResult.Should().BeEquivalentTo(ServiceResult<Player>.Failure(ServiceErrors.SystemError));
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        newShadowAccountChainHandler.Received(1).HandleError(Arg.Any<NewShadowAccountChainParam>());
        _logAggregate.Received(1).AppendLog("Started processing...");
        _logAggregate.Received(1).AppendStepLog($"START PROCESS \"{chainName}\"");
        _logAggregate.Received(1).AppendLog("Finished processing...");
    }
}