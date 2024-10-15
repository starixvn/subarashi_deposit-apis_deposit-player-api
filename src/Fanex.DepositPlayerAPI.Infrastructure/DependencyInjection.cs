namespace Fanex.DepositPlayerAPI.Infrastructure;

using DbSettingConfig;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Repositories;
using Repositories.Decorate;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.TryAddSingleton<IShardDbInfoAccessor, ShardDbInfoAccessor>();

        services.AddMemoryCache(options => { options.TrackStatistics = true; });

        // repositories
        services.TryAddTransient<IRollbackRepository, RollbackRepository>();
        services.TryAddTransient<IPlayerRepository, PlayerRepository>();
        services.TryAddTransient<IAffiliateRepository, AffiliateRepository>();
        services.TryAddTransient<IBalanceRepository, BalanceRepository>();
        services.TryAddTransient<ICommissionRepository, CommissionRepository>();
        services.TryAddTransient<ILogDbRepository, LogDbRepository>();
        services.TryAddSingleton<IDbFactory, DbFactory>();

        services.AddDecorateRepository();

        return services;
    }

    private static void AddDecorateRepository(this IServiceCollection services)
    {
        services.TryAddScoped<IServerDbConfigRepository, ServerDbConfigRepository>();
        services.TryDecorate<IServerDbConfigRepository, CachedServerDbConfigRepository>();
    }
}