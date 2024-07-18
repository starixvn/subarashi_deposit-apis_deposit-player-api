namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;
using Params;
using Results;

public interface IBalanceRepository
{
    Task<ServiceResult> InitializeNewPlayerCreditBalanceAsync(int custId);

    Task<ServiceResult> InitializeNewPlayerDebitBalanceAsync(int custId);

    Task<ServiceResult<LogTracking>> InitializeReactivatePlayerBalanceAsync(InitializeReactivatePlayerBalanceParam param);
}