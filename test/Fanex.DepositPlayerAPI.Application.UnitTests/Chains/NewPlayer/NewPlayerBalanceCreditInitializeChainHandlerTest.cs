namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewPlayerBalanceCreditInitializeChainHandlerTest
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly NewPlayerBalanceCreditInitializeChainHandler _handler;
    private readonly ILogAggregate _logAggregate;
    private readonly ILogger _logger;

    public NewPlayerBalanceCreditInitializeChainHandlerTest()
    {
        _logger = Substitute.For<ILogger>();
        _logAggregate = Substitute.For<ILogAggregate>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _handler = new NewPlayerBalanceCreditInitializeChainHandler(_logger, _logAggregate, _balanceRepository, _backgroundTask);
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
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenInitializeNewPlayerCreditBalanceFailureGracefully(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = 123;
        var extraInfo = "Initialize New Player Credit Balance failed";
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
        _logger.Received(1).Error(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeNewPlayerCreditBalanceSuccessCorrectly()
    {
        // Arrange
        var custId = 123;
        var extraInfo = "Initialize New Player Credit Balance success";
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
        _logger.DidNotReceive().Error(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInitializeNewPlayerCreditBalanceThrowException()
    {
        // Arrange
        var custId = 123;
        var extraInfo = "Initialize New Player Credit Balance failed";
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerCreditBalanceAsync(custId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(ServiceErrors.SystemError);
        await _balanceRepository.Received(1).InitializeNewPlayerCreditBalanceAsync(custId);
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }
}