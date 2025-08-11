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
            ? $"ERROR [Background Task]: An exception occurred in the {processName} background process."
            : $"ERROR: An exception occurred in the {processName} process.");
        messageBuilder.AppendLine($"FUNCTION: {funcName}");

        return messageBuilder.ToString();
    }

    [Fact]
    public void ExecuteWithTryCatch_Should_ReturnTrue_WhenExecuteSuccess()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var executed = false;

        // Action
        var isSuccess = ProcessUtils.ExecuteWithTryCatch(() => executed = true, funcName, processName, _logger);

        // Assert
        isSuccess.Should().BeTrue();
        executed.Should().BeTrue();
        _logger.DidNotReceiveWithAnyArgs().Error(default);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExecuteWithTryCatch_Should_ReturnFalse_WhenExecuteThrowException(bool isBackground)
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var errorMsg = GetErrorMessageBuilder(isBackground, funcName, processName);

        // Action
        var isSuccess = ProcessUtils.ExecuteWithTryCatch(() => throw new Exception(), funcName, processName, _logger, isBackground);

        // Assert
        isSuccess.Should().BeFalse();
        _logger.Received(1).Error(Arg.Is<string>(x => x.Contains(errorMsg)));
    }

    [Fact]
    public async Task ExecuteWithTryCatchAsync_Should_CallFunction_WhenNoException()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var executed = false;

        // Action
        var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        }, funcName, processName, _logger);

        // Assert
        isSuccess.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public void ExecuteWithTryCatch_Should_CallFunction_WhenNoException()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var executed = false;

        // Action
        var isSuccess = ProcessUtils.ExecuteWithTryCatch(() => executed = true, funcName, processName, _logger);

        // Assert
        isSuccess.Should().BeTrue();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteWithTryCatchAsync_Should_IncludeExceptionMessage_WhenExceptionThrown()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var exceptionMessage = _fixture.Create<string>();

        // Action
        var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => throw new InvalidOperationException(exceptionMessage), funcName, processName, _logger);

        // Assert
        isSuccess.Should().BeFalse();
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(x => x.Contains(exceptionMessage)));
    }

    [Fact]
    public void ExecuteWithTryCatch_Should_IncludeExceptionMessage_WhenExceptionThrown()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();
        var exceptionMessage = _fixture.Create<string>();

        // Action
        var isSuccess = ProcessUtils.ExecuteWithTryCatch(() => throw new InvalidOperationException(exceptionMessage), funcName, processName, _logger);

        // Assert
        isSuccess.Should().BeFalse();
        _logger.Received(1).Error(Arg.Is<string>(x => x.Contains(exceptionMessage)));
    }

    [Fact]
    public async Task ExecuteWithTryCatchAsync_Should_LogCorrectErrorFormat_ForBackgroundTask()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();

        // Action
        var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => throw new Exception(), funcName, processName, _logger, isBackground: true);

        // Assert
        isSuccess.Should().BeFalse();
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(x => x.Contains("ERROR [Background Task]:")));
    }

    [Fact]
    public void ExecuteWithTryCatch_Should_LogCorrectErrorFormat_ForBackgroundTask()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();

        // Action
        var isSuccess = ProcessUtils.ExecuteWithTryCatch(() => throw new Exception(), funcName, processName, _logger, isBackground: true);

        // Assert
        isSuccess.Should().BeFalse();
        _logger.Received(1).Error(Arg.Is<string>(x => x.Contains("ERROR [Background Task]:")));
    }

    [Fact]
    public async Task ExecuteWithTryCatchAsync_Should_LogCorrectErrorFormat_ForRegularTask()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();

        // Action
        var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => throw new Exception(), funcName, processName, _logger, isBackground: false);

        // Assert
        isSuccess.Should().BeFalse();
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(x => x.Contains("ERROR:") && !x.Contains("[Background Task]:")));
    }

    [Fact]
    public void ExecuteWithTryCatch_Should_LogCorrectErrorFormat_ForRegularTask()
    {
        // Arrange
        var funcName = _fixture.Create<string>();
        var processName = _fixture.Create<string>();

        // Action
        var isSuccess = ProcessUtils.ExecuteWithTryCatch(() => throw new Exception(), funcName, processName, _logger, isBackground: false);

        // Assert
        isSuccess.Should().BeFalse();
        _logger.Received(1).Error(Arg.Is<string>(x => x.Contains("ERROR:") && !x.Contains("[Background Task]:")));
    }
}