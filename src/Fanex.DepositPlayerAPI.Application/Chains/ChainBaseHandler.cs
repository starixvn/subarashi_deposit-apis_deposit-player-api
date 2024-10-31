namespace Fanex.DepositPlayerAPI.Application.Chains;

using System.Text;
using Domain.Errors;
using Domain.Results;
using Logging;
using Newtonsoft.Json;
using Services;
using Utils;

public abstract class ChainBaseHandler<TRequest, TResponse>(
    ILogger logger,
    ILogAggregate logAggregate) : IChainHandler<TRequest, TResponse>
    where TResponse : class
    where TRequest : class
{
    protected readonly ILogAggregate LogAggregate = logAggregate;
    protected readonly ILogger Logger = logger;

    private IChainHandler<TRequest, TResponse> _nextHandler;

    public abstract bool IsBreakOnError { get; }

    public abstract bool IsBreakOnSuccess { get; }

    public abstract string ChainName { get; }

    public abstract string FunctionName { get; }

    public IChainHandler<TRequest, TResponse> SetNextHandler(IChainHandler<TRequest, TResponse> handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public virtual async Task<ServiceResult<TResponse>> Handle(TRequest request)
    {
        var result = ServiceResult<TResponse>.Success(null);

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
                return ServiceResult<TResponse>.Failure(ServiceErrors.SystemError);
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

    public void LogErrorIfFailure(ServiceResult<TResponse> response, TRequest request)
    {
        if (response is null or { IsSuccess: true })
        {
            return;
        }

        // Log error
        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"ERROR [{FunctionName}]: {JsonConvert.SerializeObject(response.Error)}");
        messageBuilder.AppendLine($"MESSAGE: An error occurred in the {ChainName} process.");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine(response.ExtraInfo);
        messageBuilder.AppendLine();
        messageBuilder.AppendLine("REQUEST INFO:");
        messageBuilder.AppendLine(JsonConvert.SerializeObject(request, Formatting.Indented));
        messageBuilder.AppendLine();
        messageBuilder.AppendLine("RESPONSE INFO:");
        messageBuilder.AppendLine(JsonConvert.SerializeObject(response.Error, Formatting.Indented));

        Logger.Error(messageBuilder.ToString());
    }

    public abstract Task<ServiceResult<TResponse>> HandleInternal(TRequest request);

    public virtual void HandleError(TRequest request)
    {
        // No action as default
    }

    public virtual void HandleSuccess(TRequest request)
    {
        // No action as default
    }

    public virtual bool IsProcessFailure(ServiceResult<TResponse> response) => response is { IsFailure: true };

    private Task<ServiceResult<TResponse>> RunNextHandle(TRequest request, ServiceResult<TResponse> result) =>
        _nextHandler is null ? Task.FromResult(result) : _nextHandler.Handle(request);
}