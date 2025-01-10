namespace Fanex.DepositPlayerAPI.Application.Chains;

using System.Text;
using Domain.Errors;
using Domain.Results;
using Logging;
using Newtonsoft.Json;
using Services;
using Utils;

public abstract class ChainBaseNonResultHandler<TRequest>(
    ILogger logger,
    ILogAggregate logAggregate) : IChainNonResultHandler<TRequest>
    where TRequest : class
{
    protected readonly ILogAggregate LogAggregate = logAggregate;
    protected readonly ILogger Logger = logger;

    private IChainNonResultHandler<TRequest> _nextHandler;

    public abstract bool IsBreakOnError { get; }

    public abstract bool IsBreakOnSuccess { get; }

    public abstract string ChainName { get; }

    public abstract string FunctionName { get; }

    public IChainNonResultHandler<TRequest> SetNextHandler(IChainNonResultHandler<TRequest> handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public virtual async Task<ServiceResult> Handle(TRequest request)
    {
        var result = ServiceResult.Success();

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
                return ServiceResult.Failure(ServiceError.SystemError);
            }

            return await RunNextHandle(request, result);
        }

        LogAggregate.AppendLog($"Result Info: {JsonConvert.SerializeObject(result, Formatting.Indented)}");
        LogAggregate.AppendLog($"Extra Info: \n{result.ExtraInfo}");

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

    public void LogErrorIfFailure(ServiceResult response, TRequest request)
    {
        if (response is null or { IsSuccess: true })
        {
            return;
        }

        Logger.Error(ErrorMessageUtils.GenerateMessage(FunctionName, ChainName, response, request));
    }

    public abstract Task<ServiceResult> HandleInternal(TRequest request);

    public virtual void HandleError(TRequest request)
    {
        // No action as default
    }

    public virtual void HandleSuccess(TRequest request)
    {
        // No action as default
    }

    public virtual bool IsProcessFailure(ServiceResult response) => response is { IsFailure: true };

    private Task<ServiceResult> RunNextHandle(TRequest request, ServiceResult result) =>
        _nextHandler is null ? Task.FromResult(result) : _nextHandler.Handle(request);
}