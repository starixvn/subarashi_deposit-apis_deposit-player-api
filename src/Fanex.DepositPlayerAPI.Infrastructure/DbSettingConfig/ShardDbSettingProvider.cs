namespace Fanex.DepositPlayerAPI.Infrastructure.DbSettingConfig;

using Data;
using Microsoft.Data.SqlClient;

public class ShardDbSettingProvider(IShardDbInfoAccessor shardDbInfoAccessor, string resourcePath) : XmlDbSettingProvider(resourcePath)
{
    public override IDbSetting GetSetting(string key)
    {
        if (shardDbInfoAccessor.TryUseShardDb(key, out var dbServer))
        {
            var spKey = shardDbInfoAccessor.GetSpKey(key);
            var originalSetting = base.GetSetting(spKey);

            var connectionStr = new SqlConnectionStringBuilder(originalSetting.ConnectionString)
            {
                DataSource = dbServer
            };

            return new DbSetting(
                spKey,
                originalSetting.DbName,
                connectionStr.ToString(),
                originalSetting.SpName,
                originalSetting.CommandTimeout,
                originalSetting.ColumnMaps,
                originalSetting.FirmParameters);
        }

        return base.GetSetting(key);
    }
}