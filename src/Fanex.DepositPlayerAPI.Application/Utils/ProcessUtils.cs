namespace Fanex.DepositPlayerAPI.Application.Utils;

using System.Text;
using Logging;

public static class ProcessUtils
{
    public static async Task<bool> ExecuteWithTryCatchAsync(
        Func<Task> func,
        string funcName,
        string processName,
        ILogger logger,
        bool isBackground = false)
    {
        try
        {
            await func();
            return true; // success
        }
        catch (Exception exception)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(isBackground
                ? $"ERROR [Background Task]: An exception occurred in the {processName} background process."
                : $"ERROR: An exception occurred in the {processName} process.");
            messageBuilder.AppendLine($"FUNCTION: {funcName}");
            messageBuilder.AppendLine(ErrorMessageUtils.GenerateMessage(exception));
            await logger.ErrorAsync(messageBuilder.ToString());
            return false; // failure
        }
    }

    public static bool ExecuteWithTryCatch(
        Action func,
        string funcName,
        string processName,
        ILogger logger,
        bool isBackground = false)
    {
        try
        {
            func();
            return true; // success
        }
        catch (Exception exception)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(isBackground
                ? $"ERROR [Background Task]: An exception occurred in the {processName} background process."
                : $"ERROR: An exception occurred in the {processName} process.");
            messageBuilder.AppendLine($"FUNCTION: {funcName}");
            messageBuilder.AppendLine(ErrorMessageUtils.GenerateMessage(exception));
            logger.Error(messageBuilder.ToString());
            return false; // failure
        }
    }
}