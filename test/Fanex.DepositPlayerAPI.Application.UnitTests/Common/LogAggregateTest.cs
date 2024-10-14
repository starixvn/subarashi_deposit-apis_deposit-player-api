namespace Fanex.DepositPlayerAPI.Application.UnitTests.Common;

using System.Text;
using Application.Common;
using Application.Common.Interfaces;
using AutoFixture;
using FluentAssertions;
using Logging;
using NSubstitute;

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
}