namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Results;

public interface IChainNonResultHandler<TRequest> where TRequest : class
{
    IChainNonResultHandler<TRequest> SetNextHandler(IChainNonResultHandler<TRequest> handler);

    Task<ServiceResult> Handle(TRequest request);
}