namespace Fanex.DepositPlayerAPI.Application.Common;

using System.Text;
using Interfaces;
using Microsoft.Extensions.Logging;
using ILogger = Logging.ILogger;

public class LogAggregate(
    ILogger logger,
    IDateTimeProvider dateTimeProvider) : ILogAggregate
{
    private int _step;

    private StringBuilder Builder { get; set; } = new();

    public void AppendHeader(string message)
        => Builder.AppendLine($"{message.ToUpperInvariant()}");

    public void AppendLog(string message)
        => Builder.AppendLine($"{dateTimeProvider.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {message}");

    public void AppendStepLog(string message)
    {
        Builder.AppendLine($"\n- STEP {_step}: {message}");
        _step++;
    }

    public string GetAggregateMsg()
    {
        var logMsg = Builder.ToString();
        return logMsg;
    }

    public void SendLog(LogLevel logLevel)
    {
        if (Builder.Length == 0)
        {
            return;
        }

        switch (logLevel)
        {
            case LogLevel.Critical:
                logger.Critical(Builder.ToString());
                break;

            case LogLevel.Error:
                logger.Error(Builder.ToString());
                break;

            default: // Log Information
                logger.Info(Builder.ToString());
                break;
        }

        Builder = new StringBuilder();
    }
}