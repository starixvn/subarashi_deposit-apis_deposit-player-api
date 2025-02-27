namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Services;

using FluentAssertions;
using Infrastructure.Services;

public class SystemDateTimeProviderTest
{
    [Fact]
    public void UtcNow_Should_ReturnCurrentUtcDateTime()
    {
        // Arrange
        var dateTimeProvider = new SystemDateTimeProvider();
        var expectedDateTime = DateTimeOffset.UtcNow.DateTime;

        // Act
        var result = dateTimeProvider.UtcNow;

        // Assert
        result.Should().BeCloseTo(expectedDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MinValue_Should_ReturnMinDateTime()
    {
        // Arrange
        var dateTimeProvider = new SystemDateTimeProvider();
        var expectedDateTime = DateTimeOffset.MinValue.DateTime;

        // Act
        var result = dateTimeProvider.MinValue;

        // Assert
        result.Should().Be(expectedDateTime);
    }
}