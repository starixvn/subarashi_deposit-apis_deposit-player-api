namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;
using Params;
using Results;

public interface IPlayerRepository
{
    Task<ServiceResult<ValidatePlayerInfoOutputParam>> ValidatePlayerInfoAsync(ValidatePlayerInfoParam param);

    Task<ServiceResult<InsertPlayerInfoOutputParam>> InsertPlayerInfoAsync(InsertPlayerInfoParam param);

    Task<ServiceResult> VerifyUsernameExistence(string username);

    Task<ServiceResult<InsertNewPlayerSettingOutputParam>> InsertNewPlayerSettingAsync(InsertNewPlayerSettingParam param);

    Task<ServiceResult<InsertReactivePlayerSettingOutputParam>> InsertReactivePlayerSettingAsync(InsertReactivePlayerSettingParam param);

    Task<ServiceResult<GetArchivedPlayerInfoOutputParam>> GetArchivedInfoAsync(int custId);

    Task<ServiceResult<InsertShadowAccountInfoOutputParam>> InsertShadowAccountInfoAsync(InsertShadowAccountInfoParam param);

    Task<ServiceResult<LogData>> InsertShadowAccountSettingAsync(InsertShadowAccountSettingParam param);
}