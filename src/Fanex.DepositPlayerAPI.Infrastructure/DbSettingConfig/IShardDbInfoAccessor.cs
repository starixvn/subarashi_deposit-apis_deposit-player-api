namespace Fanex.DepositPlayerAPI.Infrastructure.DbSettingConfig;

public interface IShardDbInfoAccessor
{
    bool TryUseShardDb(string key, out string dbServer);

    string GetSpKey(string key);

    string ToShardCustDbSp(string key);

    string ToShardSiteDbSp(string key);
}