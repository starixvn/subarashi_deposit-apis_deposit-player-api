namespace Fanex.DepositPlayerAPI.Application.Chains;

using System.Text;
using Logging;
using Newtonsoft.Json;
using Services;
using Utils;

public abstract class ChainBaseHandlerCore<TRequest, TResult>(
    ILogger logger,
    ILogAggregate logAggregate)
    where TRequest : class
    where TResult : class
{
    protected readonly ILogAggregate LogAggregate = logAggregate;
    protected readonly ILogger Logger = logger;

    public abstract bool IsBreakOnError { get; }

    public abstract bool IsBreakOnSuccess { get; }

    public abstract string ChainName { get; }

    public abstract string FunctionName { get; }

    public virtual async Task<TResult> Handle(TRequest request)
    {
        var result = CreateSuccessResult();

        LogAggregate.AppendStepLog($"START PROCESS \"{ChainName}\"");
        LogAggregate.AppendLog($"Request Info: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

        try
        {
            result = await HandleInternal(request);
        }
        catch (Exception exception)
        {
            // Log exception
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"ERROR [{FunctionName}]: An exception occurred in the {ChainName} process.");
            messageBuilder.AppendLine(ErrorMessageUtils.GenerateMessage(exception));
            await Logger.ErrorAsync(messageBuilder.ToString());
            LogAggregate.AppendLog($"Error occurred in {ChainName} Process");
            LogAggregate.AppendLog(messageBuilder.ToString());

            // Error handle
            ProcessUtils.ExecuteWithTryCatch(() => HandleError(request), "HandleError", ChainName, Logger);

            if (IsBreakOnError)
            {
                return CreateFailureResult();
            }

            return await RunNextHandle(request, result);
        }

        LogAggregate.AppendLog($"Result Info: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        LogAggregate.AppendLog($"Extra Info: \n{GetExtraInfo(result)}");

        // Log an error
        LogErrorIfFailure(result, request);

        if (IsProcessFailure(result))
        {
            ProcessUtils.ExecuteWithTryCatch(() => HandleError(request), "HandleError", ChainName, Logger);
        }
        else
        {
            ProcessUtils.ExecuteWithTryCatch(() => HandleSuccess(request), "HandleSuccess", ChainName, Logger);
        }

        if (IsProcessFailure(result) && IsBreakOnError)
        {
            return result;
        }

        return IsBreakOnSuccess ? result : await RunNextHandle(request, result);
    }

    public abstract void LogErrorIfFailure(TResult response, TRequest request);

    public abstract Task<TResult> HandleInternal(TRequest request);

    public virtual void HandleError(TRequest request)
    {
        // No action as default
    }

    public virtual void HandleSuccess(TRequest request)
    {
        // No action as default
    }

    public abstract bool IsProcessFailure(TResult response);

    protected abstract TResult CreateSuccessResult();

    protected abstract TResult CreateFailureResult();

    protected abstract bool IsSuccess(TResult result);

    protected abstract string GetExtraInfo(TResult result);

    protected abstract Task<TResult> RunNextHandle(TRequest request, TResult result);
}