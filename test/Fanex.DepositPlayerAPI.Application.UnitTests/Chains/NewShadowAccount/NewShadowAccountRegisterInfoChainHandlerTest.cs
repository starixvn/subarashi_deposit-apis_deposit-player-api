namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewShadowAccount;

using Application.Chains.NewShadowAccount;
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

public class NewShadowAccountRegisterInfoChainHandlerTest : ChainHandlerTestBase
{
    private readonly NewShadowAccountRegisterInfoChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public NewShadowAccountRegisterInfoChainHandlerTest()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _handler = new NewShadowAccountRegisterInfoChainHandler(Logger, LogAggregate, _playerRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertShadowAccountInfoSuccess()
    {
        // Arrange
        var request = Fixture.Create<NewShadowAccountChainParam>();
        var outputParam = Fixture.Create<InsertShadowAccountInfoOutputParam>();

        _playerRepository
            .InsertShadowAccountInfoAsync(Arg.Any<InsertShadowAccountInfoParam>())
            .Returns(ServiceResult<InsertShadowAccountInfoOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        request.Username.Should().Be(outputParam.Username);
        request.Username2.Should().Be(outputParam.Username2);
        request.CustId.Should().Be(outputParam.CustId);
        await _playerRepository.Received(1).InsertShadowAccountInfoAsync(Arg.Is<InsertShadowAccountInfoParam>(x =>
            x.IsTest == request.IsTest
            && x.AgentId == request.AgentId
            && x.SiteDbGroupId == request.SiteDbGroupId));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertShadowAccountInfoFailure()
    {
        // Arrange
        var request = Fixture.Create<NewShadowAccountChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository
            .InsertShadowAccountInfoAsync(Arg.Any<InsertShadowAccountInfoParam>())
            .Returns(ServiceResult<InsertShadowAccountInfoOutputParam>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).InsertShadowAccountInfoAsync(Arg.Is<InsertShadowAccountInfoParam>(x =>
            x.IsTest == request.IsTest
            && x.AgentId == request.AgentId
            && x.SiteDbGroupId == request.SiteDbGroupId));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertShadowAccountInfoThrowException()
    {
        // Arrange
        var request = Fixture.Create<NewShadowAccountChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository.InsertShadowAccountInfoAsync(Arg.Any<InsertShadowAccountInfoParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).InsertShadowAccountInfoAsync(Arg.Is<InsertShadowAccountInfoParam>(x =>
            x.IsTest == request.IsTest
            && x.AgentId == request.AgentId
            && x.SiteDbGroupId == request.SiteDbGroupId));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}