namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Entities;
using Domain.Params.Chains;

public interface IChainHandlerFactory
{
    public IChainHandler<NewPlayerChainParam, Player> CreateNewPlayerChainHandler();

    public IChainNonResultHandler<ReactivatePlayerChainParam> CreateReactivatePlayerChainHandler();

    public IChainHandler<NewShadowAccountChainParam, ShadowAccount> CreateNewShadowAccountChainHandler();

    public IChainNonResultHandler<MapSabaIdChainParam> CreateMapSabaIdChainHandler();
}