namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Entities;
using Domain.Repositories;

public class LogDbRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : ILogDbRepository
{
    public async Task WriteLogDataDBLOGAsync(LogData logData)
    {
        const string SpKey = "WriteLog_UserLog_RunSP";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.UserId, logData.UserId);
        objectDb.SetParameterValue(DbParam.CustId, logData.CustId);
        objectDb.SetParameterValue(DbParam.ActionId, logData.ActionId);
        objectDb.SetParameterValue(DbParam.TrackingValue, logData.TrackingValue);
        objectDb.SetParameterValue(DbParam.TrackingValue2, logData.TrackingValue2);

        await objectDb.ExecuteNonQueryAsync();
    }

    public async Task WriteLogDataDBCUSTAsync(LogData logData)
    {
        const string SpKey = "WriteLog_UserLog_RunSP";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.UserId, logData.UserId);
        objectDb.SetParameterValue(DbParam.CustId, logData.CustId);
        objectDb.SetParameterValue(DbParam.ActionId, logData.ActionId);
        objectDb.SetParameterValue(DbParam.TrackingValue, logData.TrackingValue);
        objectDb.SetParameterValue(DbParam.TrackingValue2, logData.TrackingValue2);

        await objectDb.ExecuteNonQueryAsync();
    }

    public async Task WriteLogTrackingDBLOGAsync(LogTracking logTracking)
    {
        const string SpKey = "Writelog_Dep_Cust_Archive";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.CustId, logTracking.CustId);
        objectDb.SetParameterValue("Action", logTracking.Action);
        objectDb.SetParameterValue("Status", logTracking.Status);
        objectDb.SetParameterValue("Message", logTracking.Message);

        await objectDb.ExecuteNonQueryAsync();
    }
}