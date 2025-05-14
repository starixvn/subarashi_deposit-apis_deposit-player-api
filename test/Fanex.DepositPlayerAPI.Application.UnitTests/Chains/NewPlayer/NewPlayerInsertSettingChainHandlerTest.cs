namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains;
using Application.Chains.NewPlayer;
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

public class NewPlayerInsertSettingChainHandlerTest : ChainHandlerTestBase
{
    private readonly IBackgroundTask _backgroundTask;
    private readonly NewPlayerInsertSettingChainHandler _handler;
    private readonly IPlayerRepository _playerRepository;

    public NewPlayerInsertSettingChainHandlerTest()
    {
        _playerRepository = Substitute.For<IPlayerRepository>();
        _backgroundTask = Substitute.For<IBackgroundTask>();
        _handler = new NewPlayerInsertSettingChainHandler(Logger, LogAggregate, _playerRepository, _backgroundTask);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(123)]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertNewPlayerSettingSuccess(int? affiliateId)
    {
        // Arrange
        var request = Fixture
            .Build<NewPlayerChainParam>()
            .With(x => x.AffiliateId, affiliateId)
            .Create();

        var outputParam = Fixture.Create<InsertNewPlayerSettingOutputParam>();
        _playerRepository
            .InsertNewPlayerSettingAsync(Arg.Any<InsertNewPlayerSettingParam>())
            .Returns(ServiceResult<InsertNewPlayerSettingOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.Result.Should().BeNull();
        request.Currency.Should().Be(outputParam.Currency);
        request.LogData.Should().Be(outputParam.LogData);
        await _playerRepository.Received(1).InsertNewPlayerSettingAsync(Arg.Is<InsertNewPlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.Username2 == request.Username2
            && x.DepositUserId == request.DepositUserId
            && x.Recommend == request.Recommend
            && x.Password == request.Password
            && x.OddsType == request.OddsType
            && x.Country == request.Country
            && x.SubsidiaryId == request.SubsidiaryId
            && x.AffiliateId == request.AffiliateId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId
            && x.Site == request.Site));

        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
        _backgroundTask.Received(1).WriteLogData(_handler.ChainName, request.LogData);
        if (affiliateId > 0)
        {
            _backgroundTask.Received(1).InsertNewPlayerAffiliateInfo(_handler.ChainName, Arg.Is<NewPlayerAffiliateParam>(x =>
                x.CustId == request.CustId
                && x.AffiliateId == request.AffiliateId.Value
                && x.Username2 == request.Username2
                && x.Username == request.Username
                && x.DepositUserId == request.DepositUserId
                && x.Currency == request.Currency
                && x.Recommend == request.Recommend));
        }
        else
        {
            _backgroundTask.DidNotReceive().InsertNewPlayerAffiliateInfo(_handler.ChainName, Arg.Any<NewPlayerAffiliateParam>());
        }
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertNewPlayerSettingFailure()
    {
        // Arrange
        var request = Fixture.Create<NewPlayerChainParam>();
        var extraInfo = Fixture.Create<string>();

        _playerRepository.InsertNewPlayerSettingAsync(Arg.Any<InsertNewPlayerSettingParam>())
            .Returns(ServiceResult<InsertNewPlayerSettingOutputParam>.Failure(ServiceError.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceError.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).InsertNewPlayerSettingAsync(Arg.Is<InsertNewPlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.Username2 == request.Username2
            && x.DepositUserId == request.DepositUserId
            && x.Recommend == request.Recommend
            && x.Password == request.Password
            && x.OddsType == request.OddsType
            && x.Country == request.Country
            && x.SubsidiaryId == request.SubsidiaryId
            && x.AffiliateId == request.AffiliateId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId
            && x.Site == request.Site));

        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceError.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, request.CustId);
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), Arg.Any<LogData>());
        _backgroundTask.DidNotReceive().InsertNewPlayerAffiliateInfo(_handler.ChainName, Arg.Any<NewPlayerAffiliateParam>());
    }

    [Theory]
    [InlineData(950, "Test1")]
    [InlineData(952, "Test2")]
    [InlineData(953, "Test3")]
    public async Task ChainHandler_Should_CallNextHandler_WhenInsertNewPlayerSettingFailureWithExcludeErrorCode(int errorCode, string errorMessage)
    {
        // Arrange
        var request = Fixture
            .Build<NewPlayerChainParam>()
            .With(x => x.AffiliateId, 123)
            .Create();
        var outputParam = Fixture.Create<InsertNewPlayerSettingOutputParam>();
        var error = ServiceError.GetFromDbError(errorCode, errorMessage);
        var extraInfo = Fixture.Create<string>();
        var data = Fixture.Create<Player>();
        _playerRepository.InsertNewPlayerSettingAsync(Arg.Any<InsertNewPlayerSettingParam>())
            .Returns(ServiceResult<InsertNewPlayerSettingOutputParam>.FailureWithData(error, outputParam, extraInfo));

        var nextHandler = Substitute.For<IChainHandler<NewPlayerChainParam, Player>>();
        nextHandler.Handle(Arg.Any<NewPlayerChainParam>()).Returns(ServiceResult<Player>.Success(data, extraInfo));
        _handler.SetNextHandler(nextHandler);

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().Be(ServiceError.None);
        result.ExtraInfo.Should().Be(extraInfo);
        result.Result.Should().BeEquivalentTo(data);
        request.Currency.Should().Be(outputParam.Currency);
        request.LogData.Should().Be(outputParam.LogData);
        await nextHandler.Received(1).Handle(Arg.Any<NewPlayerChainParam>());
        await _playerRepository.Received(1).InsertNewPlayerSettingAsync(Arg.Is<InsertNewPlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.Username2 == request.Username2
            && x.DepositUserId == request.DepositUserId
            && x.Recommend == request.Recommend
            && x.Password == request.Password
            && x.OddsType == request.OddsType
            && x.Country == request.Country
            && x.SubsidiaryId == request.SubsidiaryId
            && x.AffiliateId == request.AffiliateId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId
            && x.Site == request.Site));

        Logger.Received(1).Error(Arg.Is<string>(x => x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(error)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.DidNotReceive().RollbackFullNewPlayer(Arg.Any<string>(), Arg.Any<int>());
        _backgroundTask.Received(1).WriteLogData(_handler.ChainName, request.LogData);
        _backgroundTask.Received(1).InsertNewPlayerAffiliateInfo(_handler.ChainName, Arg.Is<NewPlayerAffiliateParam>(x =>
            x.CustId == request.CustId
            && x.AffiliateId == request.AffiliateId.Value
            && x.Username2 == request.Username2
            && x.Username == request.Username
            && x.DepositUserId == request.DepositUserId
            && x.Currency == request.Currency
            && x.Recommend == request.Recommend));
    }


    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertNewPlayerSettingThrowException()
    {
        // Arrange
        var request = Fixture.Create<NewPlayerChainParam>();

        _playerRepository.InsertNewPlayerSettingAsync(Arg.Any<InsertNewPlayerSettingParam>()).Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceError.SystemError : ServiceError.None);
        result.Result.Should().BeNull();
        await _playerRepository.Received(1).InsertNewPlayerSettingAsync(Arg.Is<InsertNewPlayerSettingParam>(x =>
            x.CustId == request.CustId
            && x.Username == request.Username
            && x.Username2 == request.Username2
            && x.DepositUserId == request.DepositUserId
            && x.Recommend == request.Recommend
            && x.Password == request.Password
            && x.OddsType == request.OddsType
            && x.Country == request.Country
            && x.SubsidiaryId == request.SubsidiaryId
            && x.AffiliateId == request.AffiliateId
            && x.DefaultCommissions == request.DefaultCommissions
            && x.GroupDiscountId == request.GroupDiscountId
            && x.Site == request.Site));

        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
        _backgroundTask.Received(1).RollbackFullNewPlayer(_handler.ChainName, request.CustId);
        _backgroundTask.DidNotReceive().WriteLogData(Arg.Any<string>(), Arg.Any<LogData>());
        _backgroundTask.DidNotReceive().InsertNewPlayerAffiliateInfo(_handler.ChainName, Arg.Any<NewPlayerAffiliateParam>());
    }
}