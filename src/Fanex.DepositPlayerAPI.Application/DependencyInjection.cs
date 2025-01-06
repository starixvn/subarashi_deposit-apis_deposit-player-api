namespace Fanex.DepositPlayerAPI.Application;

using Chains;
using Chains.MapSabaId;
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
                NewPlayerGetServerDbConfigChainHandler>(nameof(NewPlayerGetServerDbConfigChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerAffiliateValidationChainHandler>(nameof(NewPlayerAffiliateValidationChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerBalanceCreditInitializeChainHandler>(nameof(NewPlayerBalanceCreditInitializeChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerBalanceDebitInitializeChainHandler>(nameof(NewPlayerBalanceDebitInitializeChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInfoValidationChainHandler>(nameof(NewPlayerInfoValidationChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertDefaultCommChainHandler>(nameof(NewPlayerInsertDefaultCommChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertDefaultGroupCommChainHandler>(nameof(NewPlayerInsertDefaultGroupCommChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertInfoChainHandler>(nameof(NewPlayerInsertInfoChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewPlayerChainParam, Player>,
                NewPlayerInsertSettingChainHandler>(nameof(NewPlayerInsertSettingChainHandler));

        // reactivate player chain
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerGetArchivedInfoChainHandler>(nameof(ReactivatePlayerGetArchivedInfoChainHandler));
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerGetServerDbConfigChainHandler>(nameof(ReactivatePlayerGetServerDbConfigChainHandler));
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerInsertDefaultGroupCommChainHandler>(nameof(ReactivatePlayerInsertDefaultGroupCommChainHandler));
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerInsertSettingChainHandler>(nameof(ReactivatePlayerInsertSettingChainHandler));
        services
            .TryAddKeyedTransient<IChainNonResultHandler<ReactivatePlayerChainParam>,
                ReactivatePlayerWriteLogDataChainHandler>(nameof(ReactivatePlayerWriteLogDataChainHandler));

        // new shadow account chain
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountGetServerDbConfigChainHandler>(nameof(NewShadowAccountGetServerDbConfigChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountRegisterInfoChainHandler>(nameof(NewShadowAccountRegisterInfoChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountInsertSettingChainHandler>(nameof(NewShadowAccountInsertSettingChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountBalanceCreditInitializeChainHandler>(nameof(NewShadowAccountBalanceCreditInitializeChainHandler));
        services
            .TryAddKeyedTransient<IChainHandler<NewShadowAccountChainParam, ShadowAccount>,
                NewShadowAccountBalanceDebitInitializeChainHandler>(nameof(NewShadowAccountBalanceDebitInitializeChainHandler));

        // map saba id
        services
            .TryAddKeyedTransient<IChainNonResultHandler<MapSabaIdChainParam>,
                MapSabaIdGetServerDbConfigChainHandler>(nameof(MapSabaIdGetServerDbConfigChainHandler));

        services
            .TryAddKeyedTransient<IChainNonResultHandler<MapSabaIdChainParam>,
                MapSabaIdValidationChainHandler>(nameof(MapSabaIdValidationChainHandler));

        services
            .TryAddKeyedTransient<IChainNonResultHandler<MapSabaIdChainParam>,
                MapSabaIdInsertDataChainHandler>(nameof(MapSabaIdInsertDataChainHandler));

        return services;
    }
}