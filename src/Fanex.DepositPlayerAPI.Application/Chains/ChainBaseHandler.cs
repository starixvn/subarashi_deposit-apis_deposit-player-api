namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Errors;
using Domain.Results;
using Logging;
using Services;
using Utils;

public abstract class ChainBaseHandler<TRequest, TResponse>(
    ILogger logger,
    ILogAggregate logAggregate) : ChainBaseHandlerCore<TRequest, ServiceResult<TResponse>>(logger, logAggregate), IChainHandler<TRequest, TResponse>
    where TResponse : class
    where TRequest : class
{
    private IChainHandler<TRequest, TResponse> _nextHandler;

    public IChainHandler<TRequest, TResponse> SetNextHandler(IChainHandler<TRequest, TResponse> handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public override void LogErrorIfFailure(ServiceResult<TResponse> response, TRequest request)
    {
        if (response is null or { IsSuccess: true })
        {
            return;
        }

        Logger.Error(ErrorMessageUtils.GenerateMessage(FunctionName, ChainName, response, request));
    }

    public override bool IsProcessFailure(ServiceResult<TResponse> response) => response is { IsFailure: true };

    protected override ServiceResult<TResponse> CreateSuccessResult() => ServiceResult<TResponse>.Success(null);

    protected override ServiceResult<TResponse> CreateFailureResult() => ServiceResult<TResponse>.Failure(ServiceError.SystemError);

    protected override bool IsSuccess(ServiceResult<TResponse> result) => result is { IsSuccess: true };

    protected override string GetExtraInfo(ServiceResult<TResponse> result) => result.ExtraInfo;

    protected override Task<ServiceResult<TResponse>> RunNextHandle(TRequest request, ServiceResult<TResponse> result) =>
        _nextHandler is null ? Task.FromResult(result) : _nextHandler.Handle(request);
}