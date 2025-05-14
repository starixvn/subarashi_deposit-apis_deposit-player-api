namespace Fanex.DepositPlayerAPI.WebApi.HealthChecks;

using System.Collections.ObjectModel;
using Data.Vault;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class VaultHealthCheck(IConfiguration configuration, VaultDbAliasService vaultDbAliasService) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        IDictionary<string, object> data = new Dictionary<string, object>();
        var dbAlias = configuration.GetSection("ConnectionStrings")
            .GetChildren()
            .Select(config => config.Key.Split(".").FirstOrDefault())
            .Distinct();

        foreach (var alias in dbAlias)
        {
            data.Add(alias, vaultDbAliasService.GetIp(alias));
        }

        return Task.FromResult(HealthCheckResult.Healthy("Vault Info", new ReadOnlyDictionary<string, object>(data)));
    }
}