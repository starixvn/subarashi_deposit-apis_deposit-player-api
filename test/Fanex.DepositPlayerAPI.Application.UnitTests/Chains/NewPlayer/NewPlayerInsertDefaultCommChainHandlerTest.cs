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

public class NewPlayerInsertDefaultCommChainHandlerTest : ChainHandlerTestBase
{
    private readonly ICommissionRepository _commissionRepository;
    private readonly NewPlayerInsertDefaultCommChainHandler _handler;

    public NewPlayerInsertDefaultCommChainHandlerTest()
    {
        _commissionRepository = Substitute.For<ICommissionRepository>();
        _handler = new NewPlayerInsertDefaultCommChainHandler(Logger, LogAggregate, _commissionRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertDefaultCommSuccess()
    {
        // Arrange
        var param = Fixture
            .Build<NewPlayerChainParam>()
            .With(x => x.GroupDiscountId, 1)
            .Create();

        _commissionRepository.InsertDefaultCommAsync(Arg.Any<InsertDefaultCommissionParam>()).Returns(ServiceResult.Success());

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        await _commissionRepository.Received(1).InsertDefaultCommAsync(Arg.Is<InsertDefaultCommissionParam>(x =>
            x.CustId == param.CustId && x.Site == param.Site && x.GroupDiscountId == param.GroupDiscountId));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_Not_InsertDefaultComm_WhenGroupDiscountIdIsInvalid()
    {
        // Arrange
        var param = Fixture
            .Build<NewPlayerChainParam>()
            .Without(x => x.GroupDiscountId)
            .Create();

        _commissionRepository.InsertDefaultCommAsync(Arg.Any<InsertDefaultCommissionParam>()).Returns(ServiceResult.Success());

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        await _commissionRepository.DidNotReceive().InsertDefaultCommAsync(Arg.Is<InsertDefaultCommissionParam>(x =>
            x.CustId == param.CustId && x.Site == param.Site && x.GroupDiscountId == param.GroupDiscountId));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertDefaultCommFailure()
    {
        // Arrange
        var param = Fixture
            .Build<NewPlayerChainParam>()
            .With(x => x.GroupDiscountId, 1)
            .Create();

        var extraInfo = Fixture.Create<string>();

        _commissionRepository.InsertDefaultCommAsync(Arg.Any<InsertDefaultCommissionParam>())
            .Returns(ServiceResult.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        result.Result.Should().BeNull();
        await _commissionRepository.Received(1).InsertDefaultCommAsync(Arg.Is<InsertDefaultCommissionParam>(x =>
            x.CustId == param.CustId && x.Site == param.Site && x.GroupDiscountId == param.GroupDiscountId));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertDefaultCommThrowException()
    {
        // Arrange
        var param = Fixture
            .Build<NewPlayerChainParam>()
            .With(x => x.GroupDiscountId, 1)
            .Create();

        _commissionRepository.InsertDefaultCommAsync(Arg.Any<InsertDefaultCommissionParam>()).Throws<Exception>();
        // Action
        var result = await _handler.Handle(param);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        result.Result.Should().BeNull();
        await _commissionRepository.Received(1).InsertDefaultCommAsync(Arg.Is<InsertDefaultCommissionParam>(x =>
            x.CustId == param.CustId && x.Site == param.Site && x.GroupDiscountId == param.GroupDiscountId));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}