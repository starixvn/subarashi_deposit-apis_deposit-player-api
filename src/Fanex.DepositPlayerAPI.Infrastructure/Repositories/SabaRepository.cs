namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Utils;

public class SabaRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : ISabaRepository
{
    public async Task<ServiceResult<ValidateSabaInfoOutputParam>> ValidateSabaInfoAsync(ValidateSabaInfoParam param)
    {
        const string SpKey = "API_CustomerIns_SabaCheck";

        using var objectDb = dbFactory.GetObjectDb(SpKey);
        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.SabaId, param.SabaId);
        objectDb.SetParameterValue(DbParam.CustType, param.CustType);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);
        var result = new ValidateSabaInfoOutputParam
        {
            IsTest = Converter.ToBoolean(output[DbParam.IsTest])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<ValidateSabaInfoOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<ValidateSabaInfoOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<LogData>> MapSabaInfoAsync(MapSabaInfoParam param)
    {
        const string SpKey = "API_CustomerIns_SabaMapping";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.SabaId, param.SabaId);
        objectDb.SetParameterValue(DbParam.CustType, param.CustType);
        objectDb.SetParameterValue(DbParam.IsTest, param.IsTest);

        var logData = await objectDb.QueryEntityAsync<LogData>();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, logData);

        if (code == 0)
        {
            return ServiceResult<LogData>.Success(logData, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<LogData>.FailureWithData(error, logData, dbInfo);
    }
}