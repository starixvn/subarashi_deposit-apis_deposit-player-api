namespace Fanex.DepositPlayerAPI.Presentation.UnitTests.Utils;

using AutoFixture;
using DTOs.Request;
using FluentAssertions;
using Presentation.Utils;

public class ObjectMapperTest
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void MapNewPlayerCommand_Should_MapPropertiesCorrectly()
    {
        // Arrange
        var createPlayerRequest = _fixture.Create<CreatePlayerRequest>();

        // Action
        var newPlayerCommand = ObjectMapper.BuildNewPlayerCommand(createPlayerRequest);

        // Assert
        newPlayerCommand.DepositUserName.Should().Be(createPlayerRequest.UserName);
        newPlayerCommand.DepositUserId.Should().Be(createPlayerRequest.UserId);
        newPlayerCommand.Password.Should().Be(createPlayerRequest.Password);
        newPlayerCommand.OddsType.Should().Be(createPlayerRequest.OddsType);
        newPlayerCommand.AgentId.Should().Be(createPlayerRequest.AgentId);
        newPlayerCommand.AffiliateId.Should().Be(createPlayerRequest.AffiliateId);
        newPlayerCommand.Country.Should().Be(createPlayerRequest.Country);
        newPlayerCommand.SubsidiaryId.Should().Be(createPlayerRequest.SubsidiaryId);
    }

    [Fact]
    public void MapNewPlayerCommand_Should_ThrowErrorWhenHandleNullRequest()
    {
        // Arrange
        CreatePlayerRequest createPlayerRequest = null!;

        // Action
        Action action = () => ObjectMapper.BuildNewPlayerCommand(createPlayerRequest);

        // Assert
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void MapShadowAccountCommand_Should_MapPropertiesCorrectly()
    {
        // Arrange
        var createShadowAccountRequest = _fixture.Create<CreateShadowAccountRequest>();

        // Action
        var newShadowAccountCommand = ObjectMapper.BuildShadowAccountCommand(createShadowAccountRequest);

        // Assert
        newShadowAccountCommand.IsTest.Should().Be(createShadowAccountRequest.IsTest);
        newShadowAccountCommand.AgentId.Should().Be(createShadowAccountRequest.AgentId);
    }

    [Fact]
    public void MapShadowAccountCommand_Should_ThrowErrorWhenHandleNullRequest()
    {
        // Arrange
        CreateShadowAccountRequest createShadowAccountRequest = null!;

        // Action
        Action action = () => ObjectMapper.BuildShadowAccountCommand(createShadowAccountRequest);

        // Assert
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void MapReactivatePlayerCommand_Should_MapPropertiesCorrectly()
    {
        // Arrange
        var reactivatePlayerRequest = _fixture.Create<ReactivatePlayerRequest>();
        var custId = _fixture.Create<int>();

        // Action
        var reactivatePlayerCommand = ObjectMapper.BuildReactivatePlayerCommand(reactivatePlayerRequest, custId);

        // Assert
        reactivatePlayerCommand.CustId.Should().Be(custId);
        reactivatePlayerCommand.SubsidiaryId.Should().Be(reactivatePlayerRequest.SubsidiaryId);
    }

    [Fact]
    public void MapReactivatePlayerCommand_Should_ThrowErrorWhenHandleNullRequest()
    {
        // Arrange
        ReactivatePlayerRequest reactivatePlayerRequest = null!;

        // Action
        Action action = () => ObjectMapper.BuildReactivatePlayerCommand(reactivatePlayerRequest, _fixture.Create<int>());

        // Assert
        action.Should().Throw<Exception>();
    }
}