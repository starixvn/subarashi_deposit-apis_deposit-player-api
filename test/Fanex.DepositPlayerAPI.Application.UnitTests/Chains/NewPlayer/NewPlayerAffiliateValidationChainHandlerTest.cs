namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class NewPlayerAffiliateValidationChainHandlerTest : ChainHandlerTestBase
{
    private readonly IAffiliateRepository _affiliateRepository;
    private readonly NewPlayerAffiliateValidationChainHandler _handler;

    public NewPlayerAffiliateValidationChainHandlerTest()
    {
        _affiliateRepository = Substitute.For<IAffiliateRepository>();
        _handler = new NewPlayerAffiliateValidationChainHandler(Logger, LogAggregate, _affiliateRepository);
    }

    [Fact]
    public async Task ChainHandler_Should_CallValidateAffiliateAsync_WhenAffiliateIdIsProvided()
    {
        // Arrange
        var affiliateId = 123;
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = affiliateId };
        _affiliateRepository.ValidateAffiliateAsync(affiliateId).Returns(ServiceResult<Player>.Success(null!));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.Result.Should().BeNull();
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_NotValidateAffiliate_WhenAffiliateIdIsNotProvided()
    {
        // Arrange
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = null };

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.Result.Should().BeNull();
        await _affiliateRepository.DidNotReceive().ValidateAffiliateAsync(Arg.Any<int>());
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Theory]
    [InlineData(12, "Invalid Affiliate agent.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenAffiliateValidationFailureGracefully(int errorCode, string errorMessage)
    {
        // Arrange
        var affiliateId = 123;
        var extraInfo = Fixture.Create<string>();
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = affiliateId };
        var error = ServiceError.GetFromDbError(errorCode, errorMessage);
        _affiliateRepository.ValidateAffiliateAsync(affiliateId).Returns(ServiceResult<Player>.Failure(error, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(error);
        result.ExtraInfo.Should().Be(extraInfo);
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        Logger.Received(1).Error(Arg.Is<string>(x => x.StartsWith($"ERROR [{_handler.FunctionName}]: {JsonConvert.SerializeObject(error)}")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenAffiliateValidationSuccessCorrectly()
    {
        // Arrange
        var affiliateId = 123;
        var extraInfo = Fixture.Create<string>();
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = affiliateId };
        _affiliateRepository.ValidateAffiliateAsync(affiliateId).Returns(ServiceResult<Player>.Success(null!, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceError.None);
        result.Result.Should().BeNull();
        result.ExtraInfo.Should().Be(extraInfo);
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        Logger.DidNotReceive().Error(Arg.Any<string>());
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_LogError_WhenAffiliateValidationThrowException()
    {
        // Arrange
        var affiliateId = 123;
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = affiliateId };
        _affiliateRepository.ValidateAffiliateAsync(affiliateId).Throws<Exception>();

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().Be(!_handler.IsBreakOnError);
        result.Error.Should().BeEquivalentTo(_handler.IsBreakOnError ? ServiceError.SystemError : ServiceError.None);
        result.Result.Should().BeNull();
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        await Logger.Received(1).ErrorAsync(Arg.Is<string>(x =>
            x.StartsWith($"ERROR [{_handler.FunctionName}]: An exception occurred in the {_handler.ChainName} process.")));
        LogAggregate.Received().AppendLog(Arg.Any<string>());
        LogAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}