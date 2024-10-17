namespace Fanex.DepositPlayerAPI.Application.UnitTests.Chains.NewPlayer;

using Application.Chains.NewPlayer;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using FluentAssertions;
using Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Services;

public class NewPlayerAffiliateValidationChainHandlerTest
{
    private readonly IAffiliateRepository _affiliateRepository;
    private readonly NewPlayerAffiliateValidationChainHandler _handler;
    private readonly ILogAggregate _logAggregate;
    private readonly ILogger _logger;

    public NewPlayerAffiliateValidationChainHandlerTest()
    {
        _logger = Substitute.For<ILogger>();
        _logAggregate = Substitute.For<ILogAggregate>();
        _affiliateRepository = Substitute.For<IAffiliateRepository>();
        _handler = new NewPlayerAffiliateValidationChainHandler(_logger, _logAggregate, _affiliateRepository);
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
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
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
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        await _affiliateRepository.DidNotReceive().ValidateAffiliateAsync(Arg.Any<int>());
    }

    [Theory]
    [InlineData(12, "Invalid Affiliate agent.")]
    [InlineData(911, "Create customer failed. Please try again.")]
    public async Task ChainHandler_Should_ReturnFailure_WhenAffiliateValidationFailureGracefully(int errorCode, string errorMessage)
    {
        // Arrange
        var affiliateId = 123;
        var extraInfo = "Affiliate validation failed";
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = affiliateId };
        var error = ServiceErrors.GetFromDbError(errorCode, errorMessage);
        _affiliateRepository.ValidateAffiliateAsync(affiliateId).Returns(ServiceResult<Player>.Failure(error, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(error);
        result.ExtraInfo.Should().Be(extraInfo);
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        _logger.Received(1).Error(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }

    [Fact]
    public async Task ChainHandler_Should_ReturnSuccess_WhenAffiliateValidationSuccessCorrectly()
    {
        // Arrange
        var affiliateId = 123;
        var extraInfo = "Affiliate validation failed";
        var newPlayerChainParam = new NewPlayerChainParam { AffiliateId = affiliateId };
        _affiliateRepository.ValidateAffiliateAsync(affiliateId).Returns(ServiceResult<Player>.Success(null!, extraInfo));

        // Action
        var result = await _handler.Handle(newPlayerChainParam);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(ServiceErrors.None);
        result.Result.Should().BeNull();
        result.ExtraInfo.Should().Be(extraInfo);
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        _logger.DidNotReceive().Error(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
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
        result.IsSuccess.Should().BeFalse();
        result.Result.Should().BeNull();
        result.Error.Should().Be(ServiceErrors.SystemError);
        await _affiliateRepository.Received(1).ValidateAffiliateAsync(affiliateId);
        await _logger.Received(1).ErrorAsync(Arg.Any<string>());
        _logAggregate.Received().AppendLog(Arg.Any<string>());
        _logAggregate.Received(1).AppendStepLog(Arg.Any<string>());
    }
}