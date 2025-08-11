namespace Fanex.DepositPlayerAPI.Domain.UnitTests.Options;

using AutoFixture;
using Domain.Options;
using FluentAssertions;

public class SentrySettingTest
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void SettingName_Should_ReturnCorrectConstant()
    {
        // Act & Assert
        SentrySetting.SettingName.Should().Be("Sentry");
    }

    [Fact]
    public void Environment_Should_SetAndGetCorrectly()
    {
        // Arrange
        var sentrySetting = new SentrySetting();
        var environment = _fixture.Create<string>();

        // Act
        sentrySetting.Environment = environment;

        // Assert
        sentrySetting.Environment.Should().Be(environment);
    }

    [Fact]
    public void Dsn_Should_SetAndGetCorrectly()
    {
        // Arrange
        var sentrySetting = new SentrySetting();
        var dsn = _fixture.Create<string>();

        // Act
        sentrySetting.Dsn = dsn;

        // Assert
        sentrySetting.Dsn.Should().Be(dsn);
    }

    [Fact]
    public void MaxBreadcrumbs_Should_SetAndGetCorrectly()
    {
        // Arrange
        var sentrySetting = new SentrySetting();
        var maxBreadcrumbs = _fixture.Create<int>();

        // Act
        sentrySetting.MaxBreadcrumbs = maxBreadcrumbs;

        // Assert
        sentrySetting.MaxBreadcrumbs.Should().Be(maxBreadcrumbs);
    }

    [Fact]
    public void SendDefaultPii_Should_SetAndGetCorrectly()
    {
        // Arrange
        var sentrySetting = new SentrySetting();
        var sendDefaultPii = _fixture.Create<bool>();

        // Act
        sentrySetting.SendDefaultPii = sendDefaultPii;

        // Assert
        sentrySetting.SendDefaultPii.Should().Be(sendDefaultPii);
    }

    [Fact]
    public void SentrySetting_Should_InstantiateWithDefaultValues()
    {
        // Act
        var sentrySetting = new SentrySetting();

        // Assert
        sentrySetting.Environment.Should().BeNull();
        sentrySetting.Dsn.Should().BeNull();
        sentrySetting.MaxBreadcrumbs.Should().Be(0);
        sentrySetting.SendDefaultPii.Should().BeFalse();
    }
}