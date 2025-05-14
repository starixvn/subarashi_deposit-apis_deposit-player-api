namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Utils;

public class SabaRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : ISabaIdRepository
{
    public async Task<ServiceResult<ValidateSabaIdInfoOutputParam>> ValidateSabaIdInfoAsync(ValidateSabaIdInfoParam param)
    {
        const string SpKey = "API_CustomerIns_SabaCheck";

        using var objectDb = dbFactory.GetObjectDb(SpKey);
        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.SabaId, param.SabaId);
        objectDb.SetParameterValue(DbParam.CustType, param.CustType);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);
        var result = new ValidateSabaIdInfoOutputParam
        {
            IsTest = Converter.ToBoolean(output[DbParam.IsTest])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<ValidateSabaIdInfoOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceError.GetFromDbError(code, message);
        return ServiceResult<ValidateSabaIdInfoOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<LogData>> MapSabaIdInfoAsync(MapSabaIdInfoParam param)
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

        var error = ServiceError.GetFromDbError(code, message);
        return ServiceResult<LogData>.FailureWithData(error, logData, dbInfo);
    }
}