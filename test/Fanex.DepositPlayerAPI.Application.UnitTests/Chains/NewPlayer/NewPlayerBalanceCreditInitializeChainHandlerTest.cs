namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewPlayerBalanceCreditInitializeChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly NewPlayerBalanceCreditInitializeChainHandler _handler;

    public NewPlayerBalanceCreditInitializeChainHandlerTest()
    {
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _handler = new NewPlayerBalanceCreditInitializeChainHandler(Logger, LogAggregate, _balanceRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_CallInitializeNewPlayerCreditBalanceAsync()
    {
        // Arrange
        var custId = 123;
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Returns(ServiceResult<Player>.Success(null!));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenInitializeNewPlayerCreditBalanceFailureGracefully(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = 123;
        var extraInfo = Fixture.Create<string>();
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        var error = ServiceErrors.GetFromDbError(errorCode, errorMessage);
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Returns(ServiceResult<Player>.Failure(error, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(error);
        result.ExtraInfo.Should().Be(extraInfo);
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        Logger.Received(1).Error(Arg.Is<string>(x => x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(error)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeNewPlayerCreditBalanceSuccessCorrectly()
    {
        // Arrange
        var custId = 123;
        var extraInfo = Fixture.Create<string>();
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Returns(ServiceResult<Player>.Success(null!, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        result.ExtraInfo.Should().Be(extraInfo);
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInitializeNewPlayerCreditBalanceThrowException()
    {
        // Arrange
        var custId = 123;
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }
}