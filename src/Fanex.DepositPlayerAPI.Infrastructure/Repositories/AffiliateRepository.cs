namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories;

using Constants;
using Domain.Errors;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Utils;

public class AffiliateRepository(IDbFactory dbFactory) : IAffiliateRepository
{
    public async Task<ServiceResult> ValidateAffiliateAsync(int affiliateId)
    {
        const string SpKey = "API_CustomerIns_CheckAff";
        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue(DbParam.AffiliateId, affiliateId);

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

    public async Task InsertPlayerAffiliateAsync(NewPlayerAffiliateParam param)
    {
        const string SpKey = "Aff_CustomerInsMember";

        using var objectDb = dbFactory.GetObjectDb(SpKey);

        objectDb.SetParameterValue("IbcCustID", param.CustId);
        objectDb.SetParameterValue("AffID", param.AffiliateId);
        objectDb.SetParameterValue(DbParam.Username, param.Username2);
        objectDb.SetParameterValue("IBCUsername", param.Username);
        objectDb.SetParameterValue("DepositCustID", param.DepositUserId);
        objectDb.SetParameterValue(DbParam.Currency, param.Currency);
        objectDb.SetParameterValue(DbParam.Recommend, param.Recommend);
        objectDb.SetParameterValue("FirstName", "");
        objectDb.SetParameterValue("LastName", "");
        objectDb.SetParameterValue("Email", "");

        await objectDb.ExecuteNonQueryAsync();
    }
}