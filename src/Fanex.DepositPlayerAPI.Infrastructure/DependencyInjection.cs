namespace Fanex.DepositPlayerAPI.Infrastructure;

using Application.Services;
using DbSettingConfig;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Repositories;
using Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.TryAddSingleton<IShardDbInfoAccessor, ShardDbInfoAccessor>();

        services.AddMemoryCache(options => { options.TrackStatistics = true; });

        // services
        services.TryAddScoped<ILogAggregate, LogAggregate>();
        services.TryAddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddScoped<BackgroundTaskDependencies>();
        services.TryAddScoped<IBackgroundTask, BackgroundTask>();
        services.TryAddScoped<ICacheProvider, CacheProvider>();

        // repositories
        services.TryAddScoped<IServerDbConfigRepository, ServerDbConfigRepository>();
        services.TryAddTransient<IRollbackRepository, RollbackRepository>();
        services.TryAddTransient<IPlayerRepository, PlayerRepository>();
        services.TryAddTransient<IAffiliateRepository, AffiliateRepository>();
        services.TryAddTransient<IBalanceRepository, BalanceRepository>();
        services.TryAddTransient<ICommissionRepository, CommissionRepository>();
        services.TryAddTransient<ILogDbRepository, LogDbRepository>();
        services.TryAddTransient<ISabaIdRepository, SabaRepository>();
        services.TryAddSingleton<IDbFactory, DbFactory>();

        return services;
    }
}