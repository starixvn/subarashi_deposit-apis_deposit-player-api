namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Utils;

public class BalanceRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : IBalanceRepository
{
    public async Task<ServiceResult<LogTracking>> InitializeReactivatePlayerBalanceAsync(
        InitializeReactivatePlayerBalanceParam param)
    {
        const string SpKey = "Dot_Dep_Customer_Archive_ReactiveCustomer_Trans";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardSiteDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.CustIdBalanceDummy, param.CustIdBalanceDummy);
        objectDb.SetParameterValue(DbParam.ClosingBalance, param.ClosingBalance);

        var logTracking = await objectDb.QueryEntityAsync<LogTracking>();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, logTracking);

        if (code == 0)
        {
            return ServiceResult<LogTracking>.Success(logTracking, dbInfo);
        }

        var error = ServiceError.GetFromDbError(code, message);
        return ServiceResult<LogTracking>.FailureWithData(error, logTracking, dbInfo);
    }

    public Task<ServiceResult> InitializeNewPlayerCreditBalanceAsync(int custId)
    {
        const string SpKey = "Dot_BalanceCreditInitialize";

        return InitialNewPlayerBalanceAsync(custId, shardDbInfoAccessor.ToShardSiteDbSp(SpKey));
    }

    public Task<ServiceResult> InitializeNewPlayerDebitBalanceAsync(int custId)
    {
        const string SpKey = "Dot_BalanceDebitInitialize";

        return InitialNewPlayerBalanceAsync(custId, shardDbInfoAccessor.ToShardSiteDbSp(SpKey));
    }

    private async Task<ServiceResult> InitialNewPlayerBalanceAsync(int custId, string spKey)
    {
        using var objectDb = dbFactory.GetObjectDb(spKey);

        objectDb.SetParameterValue(DbParam.CustId, custId);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);
        var dbInfo = DbUtils.GetDatabaseInfo(objectDb);

        if (code == 0)
        {
            return ServiceResult.Success(dbInfo);
        }

        var error = ServiceError.GetFromDbError(code, message);
        return ServiceResult.Failure(error, dbInfo);
    }
}