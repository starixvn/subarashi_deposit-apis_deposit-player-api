namespace Fanex.DepositPlayerAPI.Application;

using Chains;
using Chains.NewPlayer;
using Chains.NewShadowAccount;
using Chains.ReactivatePlayer;
using Domain.Entities;
using Domain.Params.Chains;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.TryAddTransient<IChainHandlerFactory, ChainHandlerFactory>();
        // new player chain
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerGetServerDbConfigChainHandler>("NewPlayerGetServerDbConfigChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerAffiliateValidationChainHandler>("NewPlayerAffiliateValidationChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerBalanceCreditInitializeChainHandler>("NewPlayerBalanceCreditInitializeChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerBalanceDebitInitializeChainHandler>("NewPlayerBalanceDebitInitializeChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInfoValidationChainHandler>("NewPlayerInfoValidationChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertDefaultCommChainHandler>("NewPlayerInsertDefaultCommChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertDefaultGroupCommChainHandler>("NewPlayerInsertDefaultGroupCommChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertInfoChainHandler>("NewPlayerInsertInfoChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertSettingChainHandler>("NewPlayerInsertSettingChainHandler");

        // reactivate player chain
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerGetArchivedInfoChainHandler>("ReactivatePlayerGetArchivedInfoChainHandler");
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerGetServerDbConfigChainHandler>("ReactivatePlayerGetDBServerConfigChainHandler");
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerInsertDefaultGroupCommChainHandler>(
                "ReactivatePlayerInsertDefaultGroupCommChainHandler");
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerInsertSettingChainHandler>("ReactivatePlayerInsertSettingChainHandler");
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerWriteLogDataChainHandler>("ReactivatePlayerWriteLogDataChainHandler");

        // new shadow account chain
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountGetServerDbConfigChainHandler>("NewShadowAccountGetServerDbConfigChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountRegisterInfoChainHandler>("NewShadowAccountRegisterInfoChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountInsertSettingChainHandler>("NewShadowAccountInsertSettingChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountBalanceCreditInitializeChainHandler>("NewShadowAccountBalanceCreditInitializeChainHandler");
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountBalanceDebitInitializeChainHandler>("NewShadowAccountBalanceDebitInitializeChainHandler");

        return services;
    }
}