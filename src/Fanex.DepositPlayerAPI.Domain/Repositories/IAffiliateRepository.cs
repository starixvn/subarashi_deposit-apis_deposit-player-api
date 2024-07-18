namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Params;
using Results;

public interface IAffiliateRepository
{
    Task<ServiceResult> ValidateAffiliateAsync(int affiliateId);

    Task InsertPlayerAffiliateAsync(NewPlayerAffiliateParam param);
}