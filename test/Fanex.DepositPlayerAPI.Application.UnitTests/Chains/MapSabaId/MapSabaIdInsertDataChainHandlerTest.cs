namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.MapSabaId;

using Application.Chains.MapSabaId;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using Services;

public class MapSabaIdInsertDataChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly MapSabaIdInsertDataChainHandler _handler;
    private readonly ISabaRepository _sabaRepository;

    public MapSabaIdInsertDataChainHandlerTest()
    {
        _sabaRepository = Substitute.For<ISabaRepository>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _handler = new MapSabaIdInsertDataChainHandler(Logger, LogAggregate, _sabaRepository, _backgroundTask);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenMapSabaIdSuccess()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        var outputParam = Fixture.Create<LogData>();
        _sabaRepository
            .MapSabaInfoAsync(Arg.Any<MapSabaInfoParam>())
            .Returns(ServiceResult<LogData>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        request.LogData.Should().Be(outputParam);
        await _sabaRepository.Received(1).MapSabaInfoAsync(Arg.Is<MapSabaInfoParam>(x =>
            x.CustId == request.CustId
            && x.CustType == request.CustType
            && x.IsTest == request.IsTest));

        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogData(_handler.ChainName, request.LogData);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenMapSabaIdFailure()
    {
        // Arrange
        var request = Fixture.Create<MapSabaIdChainParam>();
        var extraInfo = Fixture.Create<string>();
        _sabaRepository
            .MapSabaInfoAsync(Arg.Any<MapSabaInfoParam>())
            .Returns(ServiceResult<LogData>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _sabaRepository.Received(1).MapSabaInfoAsync(Arg.Is<MapSabaInfoParam>(x =>
            x.CustId == request.CustId
            && x.CustType == request.CustType
            && x.IsTest == request.IsTest));

        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), request.LogData);
    }
}