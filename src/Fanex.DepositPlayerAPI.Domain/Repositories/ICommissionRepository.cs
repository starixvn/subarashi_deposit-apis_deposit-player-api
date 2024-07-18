namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Params;
using Results;

public interface ICommissionRepository
{
    Task<ServiceResult<InsertDefaultGroupCommissionOutputParam>> InsertDefaultGroupCommissionAsync(InsertDefaultGroupCommissionParam param);

    Task<ServiceResult> InsertDefaultCommAsync(InsertDefaultCommissionParam param);

    Task<string> GetPlayerCommProductsAsync();
}