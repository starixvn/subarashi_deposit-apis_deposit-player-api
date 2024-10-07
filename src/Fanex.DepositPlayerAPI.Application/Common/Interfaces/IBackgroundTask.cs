namespace Fanex.DepositPlayerAPI.Application.Common.Interfaces;

using Domain.Entities;
using Domain.Params;

public interface IBackgroundTask
{
    public void WriteLogData(string processName, LogData logData);

    public void WriteLogTracking(string processName, LogTracking logTracking);

    public void InsertDefaultCommLicenseeDb(string processName, InsertDefaultCommissionParam param);

    public void RollbackFullNewPlayer(string processName, int custId);

    public void RollbackInfoNewPlayer(string processName, int custId);

    public void InsertNewPlayerAffiliateInfo(string processName, NewPlayerAffiliateParam param);

    public void ReinitBalanceReactivatePlayer(string processName, InitializeReactivatePlayerBalanceParam param);
}