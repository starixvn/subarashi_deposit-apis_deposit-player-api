namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.ReactivatePlayer;

using Application.Chains.ReactivatePlayer;
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

public class ReactivatePlayerInsertDefaultGroupCommChainHandlerTest : ChainHandlerTestBase
{
    private readonly ICommissionRepository _commissionRepository;
    private readonly ReactivatePlayerInsertDefaultGroupCommChainHandler _handler;

    public ReactivatePlayerInsertDefaultGroupCommChainHandlerTest()
    {
        _commissionRepository = Substitute.For<ICommissionRepository>();
        _handler = new ReactivatePlayerInsertDefaultGroupCommChainHandler(Logger, LogAggregate, _commissionRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenInsertDefaultGroupCommissionSuccess()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var outputParam = Fixture.Create<InsertDefaultGroupCommissionOutputParam>();
        var commProducts = Fixture.Create<string>();

        _commissionRepository.GetPlayerCommProductsAsync().Returns(commProducts);

        _commissionRepository
            .InsertDefaultGroupCommissionAsync(Arg.Any<InsertDefaultGroupCommissionParam>())
            .Returns(ServiceResult<InsertDefaultGroupCommissionOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        request.DefaultCommissions.Should().Be(outputParam.DefaultCommissions);
        request.GroupDiscountId.Should().Be(outputParam.GroupDiscountId);
        await _commissionRepository.Received(1).InsertDefaultGroupCommissionAsync(Arg.Is<InsertDefaultGroupCommissionParam>(x =>
            x.CommissionProducts == commProducts && x.Site == request.Site));
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnFailure_WhenInsertDefaultGroupCommissionFailure()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var commProducts = Fixture.Create<string>();
        var extraInfo = Fixture.Create<string>();

        _commissionRepository.GetPlayerCommProductsAsync().Returns(commProducts);

        _commissionRepository
            .InsertDefaultGroupCommissionAsync(Arg.Any<InsertDefaultGroupCommissionParam>())
            .Returns(ServiceResult<InsertDefaultGroupCommissionOutputParam>.Failure(ServiceErrors.SystemError, extraInfo));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(ServiceErrors.SystemError);
        result.ExtraInfo.Should().Be(extraInfo);
        await _commissionRepository.Received(1).GetPlayerCommProductsAsync();
        await _commissionRepository.Received(1).InsertDefaultGroupCommissionAsync(Arg.Is<InsertDefaultGroupCommissionParam>(x =>
            x.CommissionProducts == commProducts && x.Site == request.Site));
        Logger.Received(1).Error(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(ServiceErrors.SystemError)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenInsertDefaultGroupCommissionThrowException()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var commProducts = Fixture.Create<string>();

        _commissionRepository.GetPlayerCommProductsAsync().Returns(commProducts);

        _commissionRepository
            .InsertDefaultGroupCommissionAsync(Arg.Any<InsertDefaultGroupCommissionParam>())
            .Throws<Exception>();

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _commissionRepository.Received(1).GetPlayerCommProductsAsync();
        await _commissionRepository.Received(1).InsertDefaultGroupCommissionAsync(Arg.Is<InsertDefaultGroupCommissionParam>(x =>
            x.CommissionProducts == commProducts && x.Site == request.Site));
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenGetPlayerCommProductsAsyncThrowException()
    {
        // Arrange
        var request = Fixture.Create<ReactivatePlayerChainParam>();
        var outputParam = Fixture.Create<InsertDefaultGroupCommissionOutputParam>();

        _commissionRepository.GetPlayerCommProductsAsync().Throws<Exception>();

        _commissionRepository
            .InsertDefaultGroupCommissionAsync(Arg.Any<InsertDefaultGroupCommissionParam>())
            .Returns(ServiceResult<InsertDefaultGroupCommissionOutputParam>.Success(outputParam));

        // Action
        var result = await _handler.Handle(request);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceErrors.SystemError : ServiceErrors.None);
        await _commissionRepository.Received(1).GetPlayerCommProductsAsync();
        await _commissionRepository.DidNotReceive().InsertDefaultGroupCommissionAsync(Arg.Any<InsertDefaultGroupCommissionParam>());
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}