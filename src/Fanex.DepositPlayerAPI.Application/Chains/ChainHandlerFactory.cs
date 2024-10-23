namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Entities;
using Domain.Params.Chains;
using Microsoft.Extensions.DependencyInjection;
using NewPlayer;
using NewShadowAccount;
using ReactivatePlayer;

public class ChainHandlerFactory(IServiceProvider serviceProvider) : IChainHandlerFactory
{
    public IChainHandler<NewPlayerChainParam, Player> CreateNewPlayerChainHandler()
    {
        var chainHandler =
            GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerGetServerDbConfigChainHandler)); // step 1 & 1.1

        // gdoc https://docs.google.com/spreadsheets/d/14c4Qycxbl88qajPd1g58arxY27kNvOUo1BIK4-XYvfc/edit?gid=494519198#gid=494519198
        chainHandler
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerAffiliateValidationChainHandler))) // step 2.1
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerInfoValidationChainHandler))) // step 2.2
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerInsertDefaultGroupCommChainHandler))) // step 2.3 & 2.4
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerInsertInfoChainHandler))) // step 3
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerInsertSettingChainHandler))) // step 4
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerInsertDefaultCommChainHandler))) // step 6
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerBalanceCreditInitializeChainHandler))) // step 7.1
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(nameof(NewPlayerBalanceDebitInitializeChainHandler))); // step 7.2

        return chainHandler;
    }

    public IChainNonResultHandler<ReactivatePlayerChainParam> CreateReactivatePlayerChainHandler()
    {
        var chainHandler =
            GetChainNonResultHandler<ReactivatePlayerChainParam>(nameof(ReactivatePlayerWriteLogDataChainHandler)); // step 1

        // gdoc https://docs.google.com/spreadsheets/d/14c4Qycxbl88qajPd1g58arxY27kNvOUo1BIK4-XYvfc/edit?gid=494519198#gid=494519198
        chainHandler
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(nameof(ReactivatePlayerGetArchivedInfoChainHandler))) // step 2
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(nameof(ReactivatePlayerGetServerDbConfigChainHandler))) // step 3
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(nameof(ReactivatePlayerInsertDefaultGroupCommChainHandler))) // step 5.1 & 5.2
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(nameof(ReactivatePlayerInsertSettingChainHandler))); // step 6

        return chainHandler;
    }

    public IChainHandler<NewShadowAccountChainParam, ShadowAccount> CreateNewShadowAccountChainHandler()
    {
        var chainHandler =
            GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(nameof(NewShadowAccountGetServerDbConfigChainHandler)); // step 1 & 1.1

        // gdoc https://docs.google.com/spreadsheets/d/14c4Qycxbl88qajPd1g58arxY27kNvOUo1BIK4-XYvfc/edit?gid=494519198#gid=494519198
        chainHandler
            .SetNextHandler(GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(nameof(NewShadowAccountRegisterInfoChainHandler))) // step 3
            .SetNextHandler(
                GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(nameof(NewShadowAccountInsertSettingChainHandler))) // step 4
            .SetNextHandler(
                GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(nameof(NewShadowAccountBalanceCreditInitializeChainHandler))) // step 5.1
            .SetNextHandler(
                GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(nameof(NewShadowAccountBalanceDebitInitializeChainHandler))); // step 5.2

        return chainHandler;
    }

    public IChainHandler<TRequest, TResponse> GetChainHandler<TRequest, TResponse>(string chainName)
        where TRequest : class where TResponse : class
    {
        return serviceProvider.GetRequiredKeyedService<IChainHandler<TRequest, TResponse>>(chainName);
    }

    public IChainNonResultHandler<TRequest> GetChainNonResultHandler<TRequest>(string chainName) where TRequest : class
    {
        return serviceProvider.GetRequiredKeyedService<IChainNonResultHandler<TRequest>>(chainName);
    }
}