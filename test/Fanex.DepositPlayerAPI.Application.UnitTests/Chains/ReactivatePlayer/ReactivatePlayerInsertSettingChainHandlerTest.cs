namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.ReactivatePlayer;

using Application.Chains.ReactivatePlayer;
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
using NSubstitute.ExceptionExtensions;
using Services;

public class ReactivatePlayerInsertSettingChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly ReactivatePlayerInsertSettingChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public ReactivatePlayerInsertSettingChainHandlerTest()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _handler = new ReactivatePlayerInsertSettingChainHandler(Logger, LogAggregate, _playerRepository, _backgroundTask);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(123)]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertReactivePlayerSettingSuccess(int? groupDiscountId)
    {
        // Arrange
        var request = Fixture
            .Build<ReactivatePlayerChainParam>()
            .With(x => x.GroupDiscountId, groupDiscountId)
            .Create();

        var outputParam = Fixture.Create<InsertReactivePlayerSettingOutputParam>();
        _playerRepository
            .InsertReactivePlayerSettingAsync(Arg.Any<InsertReactivePlayerSettingParam>())
            .Returns(ServiceResult<InsertReactivePlayerSettingOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        request.CustIdBalanceDummy.Should().Be(outputParam.CustIdBalanceDummy);
        request.ClosingBalance.Should().Be(outputParam.ClosingBalance);
        request.LogTracking.Should().Be(outputParam.LogTracking);
        request.LogData.Should().Be(outputParam.LogData);
        await _playerRepository.Received(1).InsertReactivePlayerSettingAsync(Arg.Is<InsertReactivePlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.CustomerInfo == request.CustomerInfo
            && x.SubsidiaryId == request.SubsidiaryId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId));

        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogData(_handler.ChainName, request.LogData);
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
        _backgroundTask.Received(1).ReinitBalanceReactivatePlayer(_handler.ChainName, Arg.Is<InitializeReactivatePlayerBalanceParam>(x =>
            x.CustId == request.CustId
            && x.CustIdBalanceDummy == request.CustIdBalanceDummy
            && x.ClosingBalance == request.ClosingBalance));

        if (groupDiscountId.HasValue && groupDiscountId.Value != 0)
        {
            _backgroundTask.Received(1).InsertDefaultCommLicenseeDb(_handler.ChainName, Arg.Is<InsertDefaultCommissionParam>(x =>
                x.Site == request.Site
                && x.CustId == request.CustId
                && x.GroupDiscountId == request.GroupDiscountId));
        }
        else
        {
            _backgroundTask.DidNotReceive().InsertDefaultCommLicenseeDb(_handler.ChainName, Arg.Any<InsertDefaultCommissionParam>());
        }
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertReactivePlayerSettingFailure()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository.InsertReactivePlayerSettingAsync(Arg.Any<InsertReactivePlayerSettingParam>())
            .Returns(ServiceResult<InsertReactivePlayerSettingOutputParam>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _playerRepository.Received(1).InsertReactivePlayerSettingAsync(Arg.Is<InsertReactivePlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.CustomerInfo == request.CustomerInfo
            && x.SubsidiaryId == request.SubsidiaryId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId));

        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), request.LogData);
        _backgroundTask.DidNotReceive().ReinitBalanceReactivatePlayer(Arg.Any<string>(), Arg.Any<InitializeReactivatePlayerBalanceParam>());
        _backgroundTask.DidNotReceive().InsertDefaultCommLicenseeDb(Arg.Any<string>(), Arg.Any<InsertDefaultCommissionParam>());
    }

    [Theory]
    [InlineData(950, "Test1")]
    [InlineData(952, "Test2")]
    [InlineData(953, "Test3")]
    public async Task ChainHandler_Should_NotCallHandleError_WhenInsertReactivePlayerSettingFailureWithExcludeErrorCode(
        int errorCode,
        string errorMessage)
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var outputParam = Fixture.Create<InsertReactivePlayerSettingOutputParam>();
        var error = ServiceErrors.GetFromDbError(errorCode, errorMessage);
        var extraInfo = Fixture.Create<string>();
        var data = Fixture.Create<Player>();
        _playerRepository.InsertReactivePlayerSettingAsync(Arg.Any<InsertReactivePlayerSettingParam>())
            .Returns(ServiceResult<InsertReactivePlayerSettingOutputParam>.FailureWithData(error, outputParam, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(error);
        result.ExtraInfo.Should().Be(extraInfo);
        await _playerRepository.Received(1).InsertReactivePlayerSettingAsync(Arg.Is<InsertReactivePlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.CustomerInfo == request.CustomerInfo
            && x.SubsidiaryId == request.SubsidiaryId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId));

        Logger.Received(1).Error(Arg.Is<string>(x => x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(error)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogData(_handler.ChainName, request.LogData);
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
        _backgroundTask.Received(1).ReinitBalanceReactivatePlayer(_handler.ChainName, Arg.Is<InitializeReactivatePlayerBalanceParam>(x =>
            x.CustId == request.CustId
            && x.CustIdBalanceDummy == request.CustIdBalanceDummy
            && x.ClosingBalance == request.ClosingBalance));

        _backgroundTask.Received(1).InsertDefaultCommLicenseeDb(_handler.ChainName, Arg.Is<InsertDefaultCommissionParam>(x =>
            x.Site == request.Site
            && x.CustId == request.CustId
            && x.GroupDiscountId == request.GroupDiscountId));
    }


    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertReactivePlayerSettingThrowException()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();

        _playerRepository.InsertReactivePlayerSettingAsync(Arg.Any<InsertReactivePlayerSettingParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _playerRepository.Received(1).InsertReactivePlayerSettingAsync(Arg.Is<InsertReactivePlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.CustomerInfo == request.CustomerInfo
            && x.SubsidiaryId == request.SubsidiaryId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId));

        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).WriteLogTracking(_handler.ChainName, request.LogTracking);
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), request.LogData);
        _backgroundTask.DidNotReceive().ReinitBalanceReactivatePlayer(Arg.Any<string>(), Arg.Any<InitializeReactivatePlayerBalanceParam>());
        _backgroundTask.DidNotReceive().InsertDefaultCommLicenseeDb(Arg.Any<string>(), Arg.Any<InsertDefaultCommissionParam>());
    }
}