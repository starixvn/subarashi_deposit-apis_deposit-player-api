namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Errors;
using Domain.Results;
using Logging;
using Services;
using Utils;

public abstract class ChainBaseNonResultHandler<TRequest>(
    ILogger logger,
    ILogAggregate logAggregate) : ChainBaseHandlerCore<TRequest, ServiceResult>(logger, logAggregate), IChainNonResultHandler<TRequest>
    where TRequest : class
{
    private IChainNonResultHandler<TRequest> _nextHandler;

    public IChainNonResultHandler<TRequest> SetNextHandler(IChainNonResultHandler<TRequest> handler)
    {
        _nextHandler = handler;
        return handler;
    }

    public override void LogErrorIfFailure(ServiceResult response, TRequest request)
    {
        if (response is null or { IsSuccess: true })
        {
            return;
        }

        Logger.Error(ErrorMessageUtils.GenerateMessage(FunctionName, ChainName, response, request));
    }

    public override bool IsProcessFailure(ServiceResult response) => response is { IsFailure: true };

    protected override ServiceResult CreateSuccessResult() => ServiceResult.Success();

    protected override ServiceResult CreateFailureResult() => ServiceResult.Failure(ServiceError.SystemError);

    protected override bool IsSuccess(ServiceResult result) => result is { IsSuccess: true };

    protected override string GetExtraInfo(ServiceResult result) => result.ExtraInfo;

    protected override Task<ServiceResult> RunNextHandle(TRequest request) =>
        _nextHandler is null ? Task.FromResult(ServiceResult.Success()) : _nextHandler.Handle(request);
}