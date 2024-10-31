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

public class NewPlayerBalanceDebitInitializeChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly NewPlayerBalanceDebitInitializeChainHandler _handler;

    public NewPlayerBalanceDebitInitializeChainHandlerTest()
    {
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _handler = new NewPlayerBalanceDebitInitializeChainHandler(Logger, LogAggregate, _balanceRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_CallInitializeNewPlayerDebitBalanceAsync()
    {
        // Arrange
        var data = Fixture.Create<Player>();
        var newPlayerChainParam = new NewPlayerChainParam { CustId = data.CustId, Username = data.Username };
        _balanceRepository.InitializeNewPlayerDebitBalanceAsync(data.CustId).Returns(ServiceResult<Player>.Success(null!));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeEquivalentTo(data);
        await _balanceRepository.Received(1).InitializeNewPlayerDebitBalanceAsync(data.CustId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenInitializeNewPlayerDebitBalanceFailureGracefully(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = 123;
        var extraInfo = Fixture.Create<string>();
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        var error = ServiceErrors.GetFromDbError(errorCode, errorMessage);
        _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId).Returns(ServiceResult<Player>.Failure(error, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(error);
        result.ExtraInfo.Should().Be(extraInfo);
        await _balanceRepository.Received(1).InitializeNewPlayerDebitBalanceAsync(custId);
        Logger.Received(1).Error(Arg.Is<string>(x => x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(error)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeNewPlayerDebitBalanceSuccessCorrectly()
    {
        // Arrange
        var data = Fixture.Create<Player>();
        var extraInfo = Fixture.Create<string>();
        var newPlayerChainParam = new NewPlayerChainParam { CustId = data.CustId, Username = data.Username };
        _balanceRepository.InitializeNewPlayerDebitBalanceAsync(data.CustId).Returns(ServiceResult<Player>.Success(null!, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeEquivalentTo(data);
        result.ExtraInfo.Should().Be(extraInfo);
        await _balanceRepository.Received(1).InitializeNewPlayerDebitBalanceAsync(data.CustId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInitializeNewPlayerDebitBalanceThrowException()
    {
        // Arrange
        var custId = 123;
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(ServiceErrors.SystemError);
        await _balanceRepository.Received(1).InitializeNewPlayerDebitBalanceAsync(custId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }
}