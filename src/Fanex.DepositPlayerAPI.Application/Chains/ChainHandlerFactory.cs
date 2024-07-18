namespace Fanex.DepositPlayerAPI.Application.Chains;

using Domain.Entities;
using Domain.Params.Chains;
using Microsoft.Extensions.DependencyInjection;

public class ChainHandlerFactory(IServiceProvider serviceProvider) : IChainHandlerFactory
{
    public IChainHandler<NewPlayerChainParam, Player> CreateNewPlayerChainHandler()
    {
        var chainHandler =
            GetChainHandler<NewPlayerChainParam, Player>("NewPlayerGetServerDbConfigChainHandler"); // step 1 & 1.1

        // gdoc https://docs.google.com/spreadsheets/d/14c4Qycxbl88qajPd1g58arxY27kNvOUo1BIK4-XYvfc/edit?gid=494519198#gid=494519198
        chainHandler
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerAffiliateValidationChainHandler")) // step 2.1
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerInfoValidationChainHandler")) // step 2.2
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerInsertDefaultGroupCommChainHandler")) // step 2.3 & 2.4
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerInsertInfoChainHandler")) // step 3
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerInsertSettingChainHandler")) // step 4
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerInsertDefaultCommChainHandler")) // step 6
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerBalanceCreditInitializeChainHandler")) // step 7.1
            .SetNextHandler(
                GetChainHandler<NewPlayerChainParam, Player>(
                    "NewPlayerBalanceDebitInitializeChainHandler")); // step 7.2

        return chainHandler;
    }

    public IChainNonResultHandler<ReactivatePlayerChainParam> CreateReactivatePlayerChainHandler()
    {
        var chainHandler =
            GetChainNonResultHandler<ReactivatePlayerChainParam>("ReactivatePlayerWriteLogDataChainHandler"); // step 1

        // gdoc https://docs.google.com/spreadsheets/d/14c4Qycxbl88qajPd1g58arxY27kNvOUo1BIK4-XYvfc/edit?gid=494519198#gid=494519198
        chainHandler
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(
                    "ReactivatePlayerGetArchivedInfoChainHandler")) // step 2
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(
                    "ReactivatePlayerGetDBServerConfigChainHandler")) // step 3
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(
                    "ReactivatePlayerInsertDefaultGroupCommChainHandler")) // step 5.1 & 5.2
            .SetNextHandler(
                GetChainNonResultHandler<ReactivatePlayerChainParam>(
                    "ReactivatePlayerInsertSettingChainHandler")); // step 6

        return chainHandler;
    }

    public IChainHandler<NewShadowAccountChainParam, ShadowAccount> CreateNewShadowAccountChainHandler()
    {
        var chainHandler = GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(
            "NewShadowAccountGetServerDbConfigChainHandler"); // step 1 & 1.1

        // gdoc https://docs.google.com/spreadsheets/d/14c4Qycxbl88qajPd1g58arxY27kNvOUo1BIK4-XYvfc/edit?gid=494519198#gid=494519198
        chainHandler
            .SetNextHandler(GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(
                "NewShadowAccountRegisterInfoChainHandler")) // step 3
            .SetNextHandler(
                GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(
                    "NewShadowAccountInsertSettingChainHandler")) // step 4
            .SetNextHandler(
                GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(
                    "NewShadowAccountBalanceCreditInitializeChainHandler")) // step 5.1
            .SetNextHandler(
                GetChainHandler<NewShadowAccountChainParam, ShadowAccount>(
                    "NewShadowAccountBalanceDebitInitializeChainHandler")); // step 5.2

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