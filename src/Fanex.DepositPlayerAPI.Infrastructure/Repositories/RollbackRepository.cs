namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Repositories;

public class RollbackRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : IRollbackRepository
{
    public async Task RollbackNewPlayerAsync(int custId)
    {
        const string SpKey = "Acc_CustomerIns_Rollback";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.CustId, custId);
        await objectDb.ExecuteNonQueryAsync();
    }

    public async Task RollbackNewPlayerSettingAsync(int custId)
    {
        const string SpKey = "Acc_Dep_CustomerInsSetting_Rollback";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, custId);
        objectDb.SetParameterValue("RoleId", 1);
        await objectDb.ExecuteNonQueryAsync();
    }

    public async Task RollbackNewPlayerUserInfoAsync(int custId)
    {
        const string SpKey = "Acc_Dep_CustomerInsUserInfo_Rollback";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, custId);
        await objectDb.ExecuteNonQueryAsync();
    }
}