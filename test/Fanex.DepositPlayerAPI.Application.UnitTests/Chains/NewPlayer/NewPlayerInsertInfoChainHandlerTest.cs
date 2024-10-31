namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using AutoFixture;
using Domain.Errors;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewPlayerInsertInfoChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly NewPlayerInsertInfoChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public NewPlayerInsertInfoChainHandlerTest()
    {
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _playerRepository = Substitute.For<IPlayerRepository>();
        _handler = new NewPlayerInsertInfoChainHandler(Logger, LogAggregate, _playerRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertNewPlayerInfoSuccess()
    {
        // Arrange
        var request = Fixture.Create<NewPlayerChainParam>();
        var outputParam = Fixture.Create<InsertPlayerInfoOutputParam>();

        _playerRepository
            .InsertPlayerInfoAsync(Arg.Any<InsertPlayerInfoParam>())
            .Returns(ServiceResult<InsertPlayerInfoOutputParam>.Success(outputParam));

        _playerRepository.VerifyUsernameExistence(Arg.Any<string>()).Returns(ServiceResult.Success());

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        request.Username.Should().Be(outputParam.Username);
        request.CustId.Should().Be(outputParam.CustId);
        await _playerRepository.Received(1).VerifyUsernameExistence(Arg.Is<string>(x => x == outputParam.Username));
        await _playerRepository.Received(1).InsertPlayerInfoAsync(Arg.Is<InsertPlayerInfoParam>(x =>
            x.Username2 == request.Username2
            && x.Recommend == request.Recommend
            && x.SiteDbGroupId == request.SiteDbGroupId
            && x.Site == request.Site
            && x.CurrencyName == request.CurrencyName
            && x.RetryNumber == 0));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackInfoNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)] // max retry is 4
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertNewPlayerInfoSuccessAfterXRetryAttempts(int failureCount)
    {
        // Arrange
        var count = 0;
        var request = Fixture.Create<NewPlayerChainParam>();
        var outputParam = Fixture.Create<InsertPlayerInfoOutputParam>();

        _playerRepository
            .InsertPlayerInfoAsync(Arg.Any<InsertPlayerInfoParam>())
            .Returns(ServiceResult<InsertPlayerInfoOutputParam>.Success(outputParam));

        _playerRepository.VerifyUsernameExistence(Arg.Any<string>()).Returns(_ =>
            ++count < failureCount ? ServiceResult.Failure(ServiceErrors.SystemError) : ServiceResult.Success());

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        request.Username.Should().Be(outputParam.Username);
        request.CustId.Should().Be(outputParam.CustId);
        await _playerRepository.Received(failureCount).VerifyUsernameExistence(Arg.Is<string>(x => x == outputParam.Username));
        await _playerRepository.Received(failureCount).InsertPlayerInfoAsync(Arg.Is<InsertPlayerInfoParam>(x =>
            x.Username2 == request.Username2
            && x.Recommend == request.Recommend
            && x.SiteDbGroupId == request.SiteDbGroupId
            && x.Site == request.Site
            && x.CurrencyName == request.CurrencyName));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackInfoNewPlayer(Arg.Any<string>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertNewPlayerInfoFailsAfterMaxRetryAttempts()
    {
        // Arrange
        var maxRetry = 4;
        var request = Fixture.Create<NewPlayerChainParam>();
        var outputParam = Fixture.Create<InsertPlayerInfoOutputParam>();

        _playerRepository
            .InsertPlayerInfoAsync(Arg.Any<InsertPlayerInfoParam>())
            .Returns(ServiceResult<InsertPlayerInfoOutputParam>.Success(outputParam));

        _playerRepository.VerifyUsernameExistence(Arg.Any<string>()).Returns(ServiceResult.Failure(ServiceErrors.SystemError));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(PlayerError.CreateCustomerFailed);
        result.Result.Should().BeNull();
        await _playerRepository.Received(maxRetry).VerifyUsernameExistence(Arg.Is<string>(x => x == outputParam.Username));
        await _playerRepository.Received(maxRetry).InsertPlayerInfoAsync(Arg.Is<InsertPlayerInfoParam>(x =>
            x.Username2 == request.Username2
            && x.Recommend == request.Recommend
            && x.SiteDbGroupId == request.SiteDbGroupId
            && x.Site == request.Site
            && x.CurrencyName == request.CurrencyName));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(PlayerError.CreateCustomerFailed)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackInfoNewPlayer(_handler.ChainName, request.CustId);
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertPlayerInfoThrowException()
    {
        // Arrange
        var request = Fixture.Create<NewPlayerChainParam>();

        _playerRepository.InsertPlayerInfoAsync(Arg.Any<InsertPlayerInfoParam>()).Throws<Exception>();

        _playerRepository.VerifyUsernameExistence(Arg.Any<string>()).Returns(ServiceResult.Success());

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _playerRepository.DidNotReceive().VerifyUsernameExistence(Arg.Any<string>());
        await _playerRepository.Received(1).InsertPlayerInfoAsync(Arg.Is<InsertPlayerInfoParam>(x =>
            x.Username2 == request.Username2
            && x.Recommend == request.Recommend
            && x.SiteDbGroupId == request.SiteDbGroupId
            && x.Site == request.Site
            && x.CurrencyName == request.CurrencyName));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackInfoNewPlayer(_handler.ChainName, request.CustId);
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenVerifyUsernameExistenceThrowException()
    {
        // Arrange
        var request = Fixture.Create<NewPlayerChainParam>();
        var outputParam = Fixture.Create<InsertPlayerInfoOutputParam>();

        _playerRepository
            .InsertPlayerInfoAsync(Arg.Any<InsertPlayerInfoParam>())
            .Returns(ServiceResult<InsertPlayerInfoOutputParam>.Success(outputParam));

        _playerRepository.VerifyUsernameExistence(Arg.Any<string>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).VerifyUsernameExistence(Arg.Is<string>(x => x == outputParam.Username));
        await _playerRepository.Received(1).InsertPlayerInfoAsync(Arg.Is<InsertPlayerInfoParam>(x =>
            x.Username2 == request.Username2
            && x.Recommend == request.Recommend
            && x.SiteDbGroupId == request.SiteDbGroupId
            && x.Site == request.Site
            && x.CurrencyName == request.CurrencyName));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackInfoNewPlayer(_handler.ChainName, request.CustId);
    }
}