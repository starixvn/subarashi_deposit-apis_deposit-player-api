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

public class NewPlayerBalanceDebitInitializeChainHandlerTest
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly NewPlayerBalanceDebitInitializeChainHandler _handler;
    private readonly ILogAggregate _logAggregate;
    private readonly ILogger _logger;

    public NewPlayerBalanceDebitInitializeChainHandlerTest()
    {
        _logger = Substitute.For<ILogger>();
        _logAggregate = Substitute.For<ILogAggregate>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _handler = new NewPlayerBalanceDebitInitializeChainHandler(_logger, _logAggregate, _balanceRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_CallInitializeNewPlayerDebitBalanceAsync()
    {
        // Arrange
        var data = new Player
        {
            CustId = 123,
            Username = "UserTest"
        };
        var newPlayerChainParam = new NewPlayerChainParam { CustId = data.CustId, Username = data.Username };
        _balanceRepository.InitializeNewPlayerDebitBalanceAsync(data.CustId).Returns(ServiceResult<Player>.Success(null!));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeEquivalentTo(data);
        await _balanceRepository.Received(1).InitializeNewPlayerDebitBalanceAsync(data.CustId);
    }

    [Theory]
    [InlineData(911, "Create customer failed. Please try again.")]
    [InlineData(999, "System error.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenInitializeNewPlayerDebitBalanceFailureGracefully(int errorCode, string errorMessage)
    {
        // Arrange
        var custId = 123;
        var extraInfo = "Initialize New Player Debit Balance failed";
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
        _logger.Received(1).Error(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInitializeNewPlayerDebitBalanceSuccessCorrectly()
    {
        // Arrange
        var data = new Player
        {
            CustId = 123,
            Username = "UserTest"
        };
        var extraInfo = "Initialize New Player Debit Balance success";
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
        _logger.DidNotReceive().Error(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInitializeNewPlayerDebitBalanceThrowException()
    {
        // Arrange
        var custId = 123;
        var extraInfo = "Initialize New Player Debit Balance failed";
        var newPlayerChainParam = new NewPlayerChainParam { CustId = custId };
        _balanceRepository.InitializeNewPlayerDebitBalanceAsync(custId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(ServiceErrors.SystemError);
        await _balanceRepository.Received(1).InitializeNewPlayerDebitBalanceAsync(custId);
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, custId);
    }
}