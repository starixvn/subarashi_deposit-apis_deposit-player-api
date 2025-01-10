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

public class NewPlayerInfoValidationChainHandlerTest : ChainHandlerTestBase
{
    private readonly NewPlayerInfoValidationChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public NewPlayerInfoValidationChainHandlerTest()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _handler = new NewPlayerInfoValidationChainHandler(Logger, LogAggregate, _playerRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenValidateFailure()
    {
        // Arrange
        var param = Fixture.Create<NewPlayerChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository
            .ValidatePlayerInfoAsync(Arg.Any<ValidatePlayerInfoParam>())
            .Returns(ServiceResult<ValidatePlayerInfoOutputParam>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).ValidatePlayerInfoAsync(Arg.Is<ValidatePlayerInfoParam>(
            x => x.AgentId == param.AgentId && x.DepositUserName == param.DepositUserName));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenValidateSuccess()
    {
        // Arrange
        var request = Fixture.Create<NewPlayerChainParam>();
        var outputParam = Fixture.Create<ValidatePlayerInfoOutputParam>();
        _playerRepository
            .ValidatePlayerInfoAsync(Arg.Any<ValidatePlayerInfoParam>())
            .Returns(ServiceResult<ValidatePlayerInfoOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        request.CurrencyName.Should().Be(outputParam.CurrencyName);
        request.Site.Should().Be(outputParam.Site);
        request.Username2.Should().Be(outputParam.Username2);
        request.Recommend.Should().Be(outputParam.Recommend);
        await _playerRepository.Received(1).ValidatePlayerInfoAsync(Arg.Is<ValidatePlayerInfoParam>(
            x => x.AgentId == request.AgentId && x.DepositUserName == request.DepositUserName));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenValidateThrowException()
    {
        // Arrange
        var param = Fixture.Create<NewPlayerChainParam>();
        _playerRepository.ValidatePlayerInfoAsync(Arg.Any<ValidatePlayerInfoParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).ValidatePlayerInfoAsync(Arg.Is<ValidatePlayerInfoParam>(
            x => x.AgentId == param.AgentId && x.DepositUserName == param.DepositUserName));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}