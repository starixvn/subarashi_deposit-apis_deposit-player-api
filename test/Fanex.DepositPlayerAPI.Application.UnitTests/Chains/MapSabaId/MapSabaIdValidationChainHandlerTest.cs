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
    private readonly ISabaIdRepository _sabaRepository;

    public MapSabaIdValidationChainHandlerTest()
    {
        _sabaRepository = Substitute.For<ISabaIdRepository>();
        _handler = new MapSabaIdValidationChainHandler(Logger, LogAggregate, _sabaRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenValidateFailure()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        var extraInfo = Fixture.Create<string>();

        _sabaRepository
            .ValidateSabaIdInfoAsync(Arg.Any<ValidateSabaIdInfoParam>())
            .Returns(ServiceResult<ValidateSabaIdInfoOutputParam>.Failure(ServiceError.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceError.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _sabaRepository.Received(1).ValidateSabaIdInfoAsync(Arg.Is<ValidateSabaIdInfoParam>(
            x => x.CustId == request.CustId && x.CustType == request.CustType));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceError.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenValidateSuccess()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        var outputParam = Fixture.Create<ValidateSabaIdInfoOutputParam>();
        _sabaRepository
            .ValidateSabaIdInfoAsync(Arg.Any<ValidateSabaIdInfoParam>())
            .Returns(ServiceResult<ValidateSabaIdInfoOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        request.IsTest.Should().Be(outputParam.IsTest);
        await _sabaRepository.Received(1).ValidateSabaIdInfoAsync(Arg.Is<ValidateSabaIdInfoParam>(
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
        _sabaRepository.ValidateSabaIdInfoAsync(Arg.Any<ValidateSabaIdInfoParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceError.SystemError : ServiceError.None);
        await _sabaRepository.Received(1).ValidateSabaIdInfoAsync(Arg.Is<ValidateSabaIdInfoParam>(
            x => x.CustId == request.CustId && x.CustType == request.CustType));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}