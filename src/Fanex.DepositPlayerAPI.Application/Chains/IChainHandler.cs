namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Results;

public interface IChainHandler<TRequest, TResponse> where TRequest : class where TResponse : class
{
    IChainHandler<TRequest, TResponse> SetNextHandler(IChainHandler<TRequest, TResponse> handler);

    Task<ServiceResult<TResponse>> Handle(TRequest request);
}