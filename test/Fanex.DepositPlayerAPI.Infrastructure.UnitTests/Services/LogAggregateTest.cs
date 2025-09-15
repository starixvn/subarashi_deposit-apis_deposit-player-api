namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Services;

using System.Text;
using Application.Services;
using AutoFixture;
using FluentAssertions;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ILogger = Logging.ILogger;

public class LogAggregateTest
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public void AppendHeader_Should_AddCorrectHeader()
    {
        // Arrange
        var headerText = _fixture.Create<string>();
        var expected = new StringBuilder();
        expected.AppendLine(headerText.ToUpperInvariant());
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);

        // Action
        logAggregate.AppendHeader(headerText);
        var result = logAggregate.GetAggregateMsg();

        // Assert
        result.Should().BeEquivalentTo(expected.ToString());
    }

    [Fact]
    public void AppendLog_Should_AddCorrectLog()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var utcNow = _fixture.Create<DateTime>();
        _dateTimeProvider.UtcNow.Returns(utcNow);
        List<string> expected =
        [
            utcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            message
        ];

        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);

        // Action
        logAggregate.AppendLog(message);
        var result = logAggregate.GetAggregateMsg();

        // Assert
        result.Should().ContainAll(expected);
    }

    [Fact]
    public void AppendStepLog_Should_AddCorrectStepLog()
    {
        // Arrange
        const int loopCount = 5;
        var message = _fixture.Create<string>();
        var expected = Enumerable.Range(0, loopCount).Select(step => $"- STEP {step}: {message}");

        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);

        // Action
        for (var i = 0; i < loopCount; i++)
        {
            logAggregate.AppendStepLog(message);
        }

        var result = logAggregate.GetAggregateMsg();

        // Assert
        result.Should().ContainAll(expected);
    }

    [Fact]
    public void SendLog_WithCriticalLevel_Should_CallCriticalLogger()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);
        logAggregate.AppendLog(message);

        // Act
        logAggregate.SendLog(LogLevel.Critical);

        // Assert
        _logger.Received(1).Critical(Arg.Is<string>(s => s.Contains(message)));
    }

    [Fact]
    public void SendLog_WithErrorLevel_Should_CallErrorLogger()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);
        logAggregate.AppendLog(message);

        // Act
        logAggregate.SendLog(LogLevel.Error);

        // Assert
        _logger.Received(1).Error(Arg.Is<string>(s => s.Contains(message)));
    }

    [Fact]
    public void SendLog_WithInformationLevel_Should_CallInfoLogger()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);
        logAggregate.AppendLog(message);

        // Act
        logAggregate.SendLog(LogLevel.Information);

        // Assert
        _logger.Received(1).Info(Arg.Is<string>(s => s.Contains(message)));
    }

    [Fact]
    public void SendLog_WithWarningLevel_Should_CallInfoLogger()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);
        logAggregate.AppendLog(message);

        // Act
        logAggregate.SendLog(LogLevel.Warning);

        // Assert
        _logger.Received(1).Info(Arg.Is<string>(s => s.Contains(message)));
    }

    [Fact]
    public void SendLog_WithEmptyBuilder_Should_NotCallLogger()
    {
        // Arrange
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);

        // Act
        logAggregate.SendLog(LogLevel.Information);

        // Assert
        _logger.DidNotReceive().Info(Arg.Any<string>());
        _logger.DidNotReceive().Error(Arg.Any<string>());
        _logger.DidNotReceive().Critical(Arg.Any<string>());
    }

    [Fact]
    public void SendLog_Should_ClearBuilderAfterSending()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);
        logAggregate.AppendLog(message);

        // Act
        logAggregate.SendLog(LogLevel.Information);
        var result = logAggregate.GetAggregateMsg();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SendLog_Should_ResetStepCounterAfterSending()
    {
        // Arrange
        var message = _fixture.Create<string>();
        var logAggregate = new LogAggregate(_logger, _dateTimeProvider);
        logAggregate.AppendStepLog(message);
        logAggregate.SendLog(LogLevel.Information);

        // Act
        logAggregate.AppendStepLog(message);
        var result = logAggregate.GetAggregateMsg();

        // Assert
        result.Should().Contain("- STEP 0:");
    }
}