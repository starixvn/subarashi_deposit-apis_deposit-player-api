namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Data;
using DbSettingConfig;

public class DbFactory(IShardDbInfoAccessor shardDbInfoAccessor) : IDbFactory
{
    public IObjectDb GetObjectDb(string spName, int timeout = 120)
    {
        var spKey = shardDbInfoAccessor.GetSpKey(spName);

        if (shardDbInfoAccessor.TryUseShardDb(spName, out var dbServer))
        {
            ObjectDbFactory.CreateInstance(spKey, timeout, dbServer);
        }

        return ObjectDbFactory.CreateInstance(spKey, timeout);
    }
}