namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using AutoFixture;
using Domain.Errors;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewPlayerInfoValidationChainHandlerTest
{
    private readonly IFixture _fixture;
    private readonly NewPlayerInfoValidationChainHandler _handler;
    private readonly ILogAggregate _logAggregate;
    private readonly ILogger _logger;
    private readonly IPlayerRepository _playerRepository;

    public NewPlayerInfoValidationChainHandlerTest()
    {
        _fixture = new Fixture();
        _logger = Substitute.For<ILogger>();
        _logAggregate = Substitute.For<ILogAggregate>();
        _playerRepository = Substitute.For<IPlayerRepository>();
        _handler = new NewPlayerInfoValidationChainHandler(_logger, _logAggregate, _playerRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenValidateFailure()
    {
        // Arrange
        var param = _fixture.Create<NewPlayerChainParam>();
        _playerRepository
            .ValidatePlayerInfoAsync(Arg.Any<ValidatePlayerInfoParam>())
            .Returns(ServiceResult<ValidatePlayerInfoOutputParam>.Failure(ServiceErrors.SystemError));

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).ValidatePlayerInfoAsync(Arg.Is<ValidatePlayerInfoParam>(
            x => x.AgentId == param.AgentId && x.DepositUserName == param.DepositUserName));
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenValidateSuccess()
    {
        // Arrange
        var param = _fixture.Create<NewPlayerChainParam>();
        var outputParam = _fixture.Create<ValidatePlayerInfoOutputParam>();
        _playerRepository
            .ValidatePlayerInfoAsync(Arg.Any<ValidatePlayerInfoParam>())
            .Returns(ServiceResult<ValidatePlayerInfoOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        param.CurrencyName.Should().Be(outputParam.CurrencyName);
        param.Site.Should().Be(outputParam.Site);
        param.Username2.Should().Be(outputParam.Username2);
        param.Recommend.Should().Be(outputParam.Recommend);
        await _playerRepository.Received(1).ValidatePlayerInfoAsync(Arg.Is<ValidatePlayerInfoParam>(
            x => x.AgentId == param.AgentId && x.DepositUserName == param.DepositUserName));
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenValidateThrowException()
    {
        // Arrange
        var param = _fixture.Create<NewPlayerChainParam>();
        var outputParam = _fixture.Create<ValidatePlayerInfoOutputParam>();
        _playerRepository.ValidatePlayerInfoAsync(Arg.Any<ValidatePlayerInfoParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(ServiceErrors.SystemError);
        await _playerRepository.Received(1).ValidatePlayerInfoAsync(Arg.Is<ValidatePlayerInfoParam>(
            x => x.AgentId == param.AgentId && x.DepositUserName == param.DepositUserName));
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}