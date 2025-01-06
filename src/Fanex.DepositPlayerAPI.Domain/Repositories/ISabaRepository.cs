namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;
using Params;
using Results;

public interface ISabaRepository
{
    Task<ServiceResult<ValidateSabaInfoOutputParam>> ValidateSabaInfoAsync(ValidateSabaInfoParam param);

    Task<ServiceResult<LogData>> MapSabaInfoAsync(MapSabaInfoParam param);
}