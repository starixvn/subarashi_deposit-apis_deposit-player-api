namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;
using Params;
using Results;

public interface ISabaIdRepository
{
    Task<ServiceResult<ValidateSabaIdInfoOutputParam>> ValidateSabaIdInfoAsync(ValidateSabaIdInfoParam param);

    Task<ServiceResult<LogData>> MapSabaIdInfoAsync(MapSabaIdInfoParam param);
}