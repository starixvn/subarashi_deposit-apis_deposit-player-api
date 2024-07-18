namespace Fanex.DepositPlayerAPI.Domain.Repositories;

public interface IRollbackRepository
{
    Task RollbackNewPlayerAsync(int custId);

    Task RollbackNewPlayerSettingAsync(int custId);

    Task RollbackNewPlayerUserInfoAsync(int custId);
}