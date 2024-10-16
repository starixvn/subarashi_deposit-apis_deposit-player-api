namespace Fanex.DepositPlayerAPI.Infrastructure.Services;

using System.Text;
using Application.Services;
using Microsoft.Extensions.Logging;
using ILogger = Logging.ILogger;

public class LogAggregate(
    ILogger logger,
    IDateTimeProvider dateTimeProvider) : ILogAggregate
{
    private StringBuilder _builder = new();
    private int _step;

    public void AppendHeader(string message)
        => _builder.AppendLine($"{message.ToUpperInvariant()}");

    public void AppendLog(string message)
        => _builder.AppendLine($"{dateTimeProvider.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}: {message}");

    public void AppendStepLog(string message)
    {
        _builder.AppendLine($"\n- STEP {_step}: {message}");
        _step++;
    }

    public string GetAggregateMsg()
    {
        var logMsg = _builder.ToString();
        return logMsg;
    }

    public void SendLog(LogLevel logLevel)
    {
        if (_builder.Length == 0)
        {
            return;
        }

        switch (logLevel)
        {
            case LogLevel.Critical:
                logger.Critical(_builder.ToString());
                break;

            case LogLevel.Error:
                logger.Error(_builder.ToString());
                break;

            default: // Log Information
                logger.Info(_builder.ToString());
                break;
        }

        _builder = new StringBuilder();
        _step = 0;
    }
}