namespace Fanex.DepositPlayerAPI.Application.UnitTests.Utils;

using System.Text;
using Application.Utils;
using AutoFixture;
using FluentAssertions;
using Logging;
using NSubstitute;

public class ProcessUtilsTest
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger _logger = Substitute.For<ILogger>();

    [Fact]
    public async Task ExecuteWithTryCatchAsync_Should_ReturnTrue_WhenExecuteSuccess()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();

        // Action
        var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => Task.CompletedTask, funcName, processName, _logger);

        // Assert
        isSuccess.Should().BeTrue();
        await _logger.DidNotReceiveWithAnyArgs().ErrorAsync(default);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteWithTryCatchAsync_Should_ReturnFalse_WhenExecuteThrowException(bool isBackground)
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var errorMsg = GetErrorMessageBuilder(isBackground, funcName, processName);

        // Action
        var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => throw new Exception(), funcName, processName, _logger, isBackground);

        // Assert
        isSuccess.Should().BeFalse();
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(x => x.Contains(errorMsg)));
    }

    private static string GetErrorMessageBuilder(bool isBackground, string funcName, string processName)
    {
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine(isBackground
            ? $"ERROR [Background Task]: An error occurred in the {processName} background process."
            : $"ERROR: An error occurred in the {processName} process.");
        messageBuilder.AppendLine($"FUNCTION: {funcName}");

        return messageBuilder.ToString();
    }
}