namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewShadowAccount;

using Application.Chains.NewShadowAccount;
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

public class NewShadowAccountBalanceCreditInitializeChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly NewShadowAccountBalanceCreditInitializeChainHandler _handler;

    public NewShadowAccountBalanceCreditInitializeChainHandlerTest()
    {
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _handler = new NewShadowAccountBalanceCreditInitializeChainHandler(Logger, LogAggregate, _balanceRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_CallInitializeNewShadowAccountCreditBalanceAsync()
    {
        // Arrange
        var custId = 123;
        var newShadowAccountChainParam = new NewShadowAccountChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Returns(ServiceResult<ShadowAccount>.Success(null!));

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.Result.Should().BeNull();
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenInitializeNewShadowAccountCreditBalanceFailureGracefully(
        int errorCode,
        string errorMessage)
    {
        // Arrange
        var custId = 123;
        var extraInfo = Fixture.Create<string>();
        var newShadowAccountChainParam = new NewShadowAccountChainParam { CustId = custId };
        var error = ServiceError.GetFromDbError(errorCode, errorMessage);
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Returns(ServiceResult<ShadowAccount>.Failure(error, extraInfo));

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

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
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeNewShadowAccountCreditBalanceSuccessCorrectly()
    {
        // Arrange
        var custId = 123;
        var extraInfo = Fixture.Create<string>();
        var newShadowAccountChainParam = new NewShadowAccountChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Returns(ServiceResult<ShadowAccount>.Success(null!, extraInfo));

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.Result.Should().BeNull();
        result.ExtraInfo.Should().Be(extraInfo);
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInitializeNewShadowAccountCreditBalanceThrowException()
    {
        // Arrange
        var custId = 123;
        var newShadowAccountChainParam = new NewShadowAccountChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(newShadowAccountChainParam);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceError.SystemError : ServiceError.None);
        result.Result.Should().BeNull();
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }
}