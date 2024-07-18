namespace Fanex.DepositPlayerAPI.WebApi.Configurations;

using Data;
using Data.SqlServer;
using Infrastructure.DbSettingConfig;

public class FanexDataServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        var listConnectionName = configuration.GetSection("ConnectionStrings").GetChildren().Select(_ => _.Key);
        var connectionStrings = listConnectionName
            .Select(connectionName => new KeyValuePair<string, ConnectionConfiguration>(
                connectionName,
                new ConnectionConfiguration(connectionName, configuration.GetConnectionString(connectionName))))
            .ToDictionary();

        var shardDbInfoAccessor = services.BuildServiceProvider().GetRequiredService<IShardDbInfoAccessor>();

        DbSettingProviderManager
            .StartNewSession()
            .Use(connectionStrings)
            .WithSqlServer()
            .Use(new ShardDbSettingProvider(shardDbInfoAccessor, "~/FanexSpName.xml"))
            .Run();
    }
}