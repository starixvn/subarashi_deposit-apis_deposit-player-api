namespace Fanex.DepositPlayerAPI.Application.UnitTests.Utils;

using Application.Utils;
using AutoFixture;
using Commands;
using FluentAssertions;

public class ParamMapperTest
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void MapNewPlayerChainParam_Should_ReturnCorrectValue()
    {
        // Arrange
        var command = _fixture.Create<NewPlayerCommand>();

        // Action
        var param = ParamMapper.MapNewPlayerChainParam(command);

        // Assert
        param.DepositUserName.Should().Be(command.DepositUserName);
        param.DepositUserId.Should().Be(command.DepositUserId);
        param.Password.Should().Be(command.Password);
        param.OddsType.Should().Be(command.OddsType);
        param.AgentId.Should().Be(command.AgentId);
        param.AffiliateId.Should().Be(command.AffiliateId);
        param.Country.Should().Be(command.Country);
        param.SubsidiaryId.Should().Be(command.SubsidiaryId);
        param.SiteDbGroupId.Should().Be(command.SiteDbGroupId);
    }

    [Fact]
    public void MapNewShadowAccountChainParam_Should_ReturnCorrectValue()
    {
        // Arrange
        var command = _fixture.Create<NewShadowAccountCommand>();

        // Action
        var param = ParamMapper.MapNewShadowAccountChainParam(command);

        // Assert
        param.AgentId.Should().Be(command.AgentId);
        param.RealCustId.Should().Be(command.RealCustId);
        param.SiteDbGroupId.Should().Be(command.SiteDbGroupId);
    }

    [Fact]
    public void MapReactivatePlayerChainParam_Should_ReturnCorrectValue()
    {
        // Arrange
        var command = _fixture.Create<ReactivatePlayerCommand>();

        // Action
        var param = ParamMapper.MapReactivatePlayerChainParam(command);

        // Assert
        param.CustId.Should().Be(command.CustId);
        param.SubsidiaryId.Should().Be(command.SubsidiaryId);
        param.Site.Should().Be(command.Site);
    }
}