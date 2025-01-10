namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.MapSabaId;

using Application.Chains.MapSabaId;
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

public class MapSabaIdValidationChainHandlerTest : ChainHandlerTestBase
{
    private readonly MapSabaIdValidationChainHandler _handler;
    private readonly ISabaRepository _sabaRepository;

    public MapSabaIdValidationChainHandlerTest()
    {
        _sabaRepository = Substitute.For<ISabaRepository>();
        _handler = new MapSabaIdValidationChainHandler(Logger, LogAggregate, _sabaRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenValidateFailure()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        var extraInfo = Fixture.Create<string>();

        _sabaRepository
            .ValidateSabaInfoAsync(Arg.Any<ValidateSabaInfoParam>())
            .Returns(ServiceResult<ValidateSabaInfoOutputParam>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _sabaRepository.Received(1).ValidateSabaInfoAsync(Arg.Is<ValidateSabaInfoParam>(
            x => x.CustId == request.CustId && x.CustType == request.CustType));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenValidateSuccess()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        var outputParam = Fixture.Create<ValidateSabaInfoOutputParam>();
        _sabaRepository
            .ValidateSabaInfoAsync(Arg.Any<ValidateSabaInfoParam>())
            .Returns(ServiceResult<ValidateSabaInfoOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        request.IsTest.Should().Be(outputParam.IsTest);
        await _sabaRepository.Received(1).ValidateSabaInfoAsync(Arg.Is<ValidateSabaInfoParam>(
            x => x.CustId == request.CustId && x.CustType == request.CustType));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenValidateThrowException()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        _sabaRepository.ValidateSabaInfoAsync(Arg.Any<ValidateSabaInfoParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _sabaRepository.Received(1).ValidateSabaInfoAsync(Arg.Is<ValidateSabaInfoParam>(
            x => x.CustId == request.CustId && x.CustType == request.CustType));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}