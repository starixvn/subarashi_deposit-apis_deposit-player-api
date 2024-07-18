namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Utils;

public class CommissionRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : ICommissionRepository
{
    public async Task<string> GetPlayerCommProductsAsync()
    {
        const string SpKey = "Acc_Dep_GetPlayerCommProducts";
        var result = string.Empty;

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));
        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        // get output params
        result = Converter.ToString(output[DbParam.CommissionProducts]);

        return result;
    }

    public async Task<ServiceResult> InsertDefaultCommAsync(InsertDefaultCommissionParam param)
    {
        const string SpKey = "API_CustomerIns_DefaultComm_LicenseeDB";
        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.Site, param.Site);
        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.GroupDiscountId, param.GroupDiscountId);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);
        var dbInfo = DbUtils.GetDatabaseInfo(objectDb);

        if (code == 0)
        {
            return ServiceResult.Success(dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult.Failure(error, dbInfo);
    }

    public async Task<ServiceResult<InsertDefaultGroupCommissionOutputParam>> InsertDefaultGroupCommissionAsync(
        InsertDefaultGroupCommissionParam param)
    {
        const string SpKey = "API_CustomerIns_GetDefaultGroupComm";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.Site, param.Site);
        objectDb.SetParameterValue(DbParam.CommissionProducts, param.CommissionProducts);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        // get output params
        var result = new InsertDefaultGroupCommissionOutputParam
        {
            DefaultCommissions = Converter.ToString(output[DbParam.DefaultCommissions]),
            GroupDiscountId = Converter.ToInt(output[DbParam.GroupDiscountId])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<InsertDefaultGroupCommissionOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<InsertDefaultGroupCommissionOutputParam>.FailureWithData(error, result, dbInfo);
    }
}