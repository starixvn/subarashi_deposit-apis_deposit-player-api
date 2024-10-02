namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using DbSettingConfig;
using Domain.Entities;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Utils;

public class PlayerRepository(IDbFactory dbFactory, IShardDbInfoAccessor shardDbInfoAccessor) : IPlayerRepository
{
    public async Task<ServiceResult<ValidatePlayerInfoOutputParam>> ValidatePlayerInfoAsync(ValidatePlayerInfoParam param)
    {
        const string SpKey = "API_CustomerIns_Validate";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue("DepositUserName", param.DepositUserName);
        objectDb.SetParameterValue(DbParam.AgentId, param.AgentId);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        // get output params
        var result = new ValidatePlayerInfoOutputParam
        {
            CurrencyName = Converter.ToString(output[DbParam.CurrencyName]),
            Site = Converter.ToString(output[DbParam.Site]),
            Username2 = Converter.ToString(output[DbParam.Username2]),
            Recommend = Converter.ToInt(output[DbParam.Recommend])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<ValidatePlayerInfoOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<ValidatePlayerInfoOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<InsertPlayerInfoOutputParam>> InsertPlayerInfoAsync(InsertPlayerInfoParam param)
    {
        const string SpKey = "API_CustomerIns_Generate";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.Username2, param.Username2);
        objectDb.SetParameterValue(DbParam.Recommend, param.Recommend);
        objectDb.SetParameterValue(DbParam.SiteDbGroupId, param.SiteDbGroupId);
        objectDb.SetParameterValue(DbParam.Site, param.Site);
        objectDb.SetParameterValue(DbParam.CurrencyName, param.CurrencyName);
        objectDb.SetParameterValue(DbParam.RetryNumber, param.RetryNumber);
        objectDb.SetParameterValue(DbParam.CustId, param.CustId);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        // get output params
        var result = new InsertPlayerInfoOutputParam
        {
            CustId = Converter.ToInt(output[DbParam.CustId]),
            Username = Converter.ToString(output[DbParam.Username])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<InsertPlayerInfoOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<InsertPlayerInfoOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult> VerifyUsernameExistence(string username)
    {
        const string SpKey = "API_CustomerIns_CheckExists";
        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.Username, username);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();
        var dbInfo = DbUtils.GetDatabaseInfo(objectDb);

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        if (code == 0)
        {
            return ServiceResult.Success(dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult.Failure(error, dbInfo);
    }

    public async Task<ServiceResult<InsertNewPlayerSettingOutputParam>> InsertNewPlayerSettingAsync(InsertNewPlayerSettingParam param)
    {
        const string SpKey = "API_CustomerIns_Member";
        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.Username, param.Username);
        objectDb.SetParameterValue(DbParam.Username2, param.Username2);
        objectDb.SetParameterValue("DepositUserId", param.DepositUserId);
        objectDb.SetParameterValue(DbParam.Recommend, param.Recommend);
        objectDb.SetParameterValue("Password", param.Password);
        objectDb.SetParameterValue(DbParam.OddsType, param.OddsType);
        objectDb.SetParameterValue("Country", param.Country);
        objectDb.SetParameterValue(DbParam.SubsidiaryId, param.SubsidiaryId);
        objectDb.SetParameterValue(DbParam.AffiliateId, param.AffiliateId);
        objectDb.SetParameterValue(DbParam.DefaultCommissions, param.DefaultCommissions);
        objectDb.SetParameterValue(DbParam.GroupDiscountId, param.GroupDiscountId);
        objectDb.SetParameterValue(DbParam.Site, param.Site);

        var logData = await objectDb.QueryEntityAsync<LogData>();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        // get output params
        var result = new InsertNewPlayerSettingOutputParam
        {
            Currency = Converter.ToInt(output[DbParam.Currency]),
            LogData = logData
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<InsertNewPlayerSettingOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<InsertNewPlayerSettingOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<InsertReactivePlayerSettingOutputParam>> InsertReactivePlayerSettingAsync(InsertReactivePlayerSettingParam param)
    {
        const string SpKey = "API_CustomerReactive_Insert";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.CustomerInfo, param.CustomerInfo);
        objectDb.SetParameterValue(DbParam.SubsidiaryId, param.SubsidiaryId);
        objectDb.SetParameterValue(DbParam.DefaultCommissions, param.DefaultCommissions);
        objectDb.SetParameterValue(DbParam.GroupDiscountId, param.GroupDiscountId);

        var reader = await objectDb.QueryMultipleAsync();

        var logTracking = await reader.ReadAsync<LogTracking>();
        var logData = await reader.ReadAsync<LogData>();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        // get output params
        var result = new InsertReactivePlayerSettingOutputParam
        {
            LogTracking = logTracking.FirstOrDefault(),
            LogData = logData.FirstOrDefault(),
            CustIdBalanceDummy = Converter.ToInt(output[DbParam.CustIdBalanceDummy]),
            ClosingBalance = Converter.ToDecimal(output[DbParam.ClosingBalance])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<InsertReactivePlayerSettingOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<InsertReactivePlayerSettingOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<GetArchivedPlayerInfoOutputParam>> GetArchivedInfoAsync(int custId)
    {
        const string SpKey = "API_CustomerReactive_GetArchivedInfo";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.CustId, custId);

        var logTracking = await objectDb.QueryEntityAsync<LogTracking>();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        // get output params
        var result = new GetArchivedPlayerInfoOutputParam
        {
            LogTracking = logTracking,
            CustomerInfo = Converter.ToString(output[DbParam.CustomerInfo]),
            Recommend = Converter.ToInt(output[DbParam.Recommend])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<GetArchivedPlayerInfoOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<GetArchivedPlayerInfoOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<InsertShadowAccountInfoOutputParam>> InsertShadowAccountInfoAsync(InsertShadowAccountInfoParam param)
    {
        const string SpKey = "API_CustomerIns_Shadow_Register";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.RealCustId, param.RealCustId);
        objectDb.SetParameterValue(DbParam.AgentId, param.AgentId);
        objectDb.SetParameterValue(DbParam.SiteDbGroupId, param.SiteDbGroupId);

        await objectDb.ExecuteNonQueryAsync();

        var output = objectDb.GetOutputParams();

        var (code, message) = DbUtils.GetErrorOutputParameters(output);

        var result = new InsertShadowAccountInfoOutputParam
        {
            Username = Converter.ToString(output[DbParam.Username]),
            Username2 = Converter.ToString(output[DbParam.Username2]),
            CustId = Converter.ToInt(output[DbParam.CustId])
        };

        var dbInfo = DbUtils.GetDatabaseInfo(objectDb, result);

        if (code == 0)
        {
            return ServiceResult<InsertShadowAccountInfoOutputParam>.Success(result, dbInfo);
        }

        var error = ServiceErrors.GetFromDbError(code, message);
        return ServiceResult<InsertShadowAccountInfoOutputParam>.FailureWithData(error, result, dbInfo);
    }

    public async Task<ServiceResult<LogData>> InsertShadowAccountSettingAsync(InsertShadowAccountSettingParam param)
    {
        const string SpKey = "Dot_Dep_CustomerInsMember_Ag_Cust_Shadow";

        using var objectDb = dbFactory.GetObjectDb(shardDbInfoAccessor.ToShardCustDbSp(SpKey));

        objectDb.SetParameterValue(DbParam.CustId, param.CustId);
        objectDb.SetParameterValue(DbParam.Username, param.Username);
        objectDb.SetParameterValue(DbParam.Username2, param.Username2);
        objectDb.SetParameterValue(DbParam.RealCustId, param.RealCustId);
        objectDb.SetParameterValue(DbParam.AgentId, param.AgentId);

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