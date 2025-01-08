namespace Fanex.DepositPlayerAPI.WebApi.Configurations;

using Data;
using Data.SqlServer;
using Data.Vault;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class FanexDataServiceInstaller : IServiceInstaller
{
    private const string SqlProviderName = "Microsoft.Data.SqlClient";

    private static readonly CreateObjectDbFunc ObjectDbFactoryFunc =
        (settingKey, commandTimeout, serverIp) => new SqlObjectDb(settingKey, commandTimeout, serverIp);

    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration
            .GetSection("ConnectionStrings")
            .GetChildren()
            .Select(_ => _.Key)
            .ToDictionary(alias => alias, alias => new ConnectionConfiguration(alias, configuration.GetConnectionString(alias), SqlProviderName));

        var vaultOptions = configuration.GetSection("VaultOptions").Get<VaultOptions>();

        DbSettingProviderManager
            .StartNewSession()
            .Use(connectionStrings)
            .Use(vaultOptions)
            .Register(SqlProviderName, ObjectDbFactoryFunc)
            .UseDefaultDbSettingProvider("~/FanexSpName.xml")
            .Run();

        services.TryAddSingleton<VaultDbAliasService, VaultDbAliasServiceImp>();
    }
}