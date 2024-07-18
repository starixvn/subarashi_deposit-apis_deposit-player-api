namespace Fanex.DepositPlayerAPI.Application.Common.Interfaces;

using Microsoft.Extensions.Logging;

public interface ILogAggregate
{
    void AppendHeader(string message);

    void AppendLog(string message);

    void AppendStepLog(string message);

    string GetAggregateMsg();

    void SendLog(LogLevel logLevel);
}