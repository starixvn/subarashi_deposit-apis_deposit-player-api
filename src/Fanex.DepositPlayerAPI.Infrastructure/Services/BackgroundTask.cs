namespace Fanex.DepositPlayerAPI.Infrastructure.Services;

using System.Text;
using Application.Services;
using Application.Utils;
using Domain.Entities;
using Domain.Params;
using Domain.Repositories;
using Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public class BackgroundTask(
    IRollbackRepository rollbackRepository,
    ILogDbRepository logDbRepository,
    IAffiliateRepository affiliateRepository,
    ICommissionRepository commissionRepository,
    IBalanceRepository balanceRepository,
    ILogger logger,
    IServiceScopeFactory serviceScopeFactory,
    IHttpContextAccessor httpContextAccessor) : IBackgroundTask
{
    private ServerDbConfig ServerDbConfig => ServerDbConfigUtils.GetServerDbConfigFromHttpContext(httpContextAccessor);

    public void WriteLogData(string processName, LogData logData)
    {
        async Task Func()
        {
            var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => logDbRepository.WriteLogDataDBLOGAsync(logData),
                "WriteLogDataDBLOGAsync", processName, logger, true);

            if (!isSuccess)
            {
                await ProcessUtils.ExecuteWithTryCatchAsync(() => logDbRepository.WriteLogDataDBCUSTAsync(logData),
                    "WriteLogDataDBCUSTAsync", processName, logger, true);
            }
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void WriteLogTracking(string processName, LogTracking logTracking)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => logDbRepository.WriteLogTrackingDBLOGAsync(logTracking),
                "WriteLogTrackingDBLOGAsync", processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void InsertDefaultCommLicenseeDb(string processName, InsertDefaultCommissionParam param)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => commissionRepository.InsertDefaultCommAsync(param),
                "InsertDefaultCommAsync", processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void RollbackFullNewPlayer(string processName, int custId)
    {
        if (!IsValidCustId(custId))
        {
            return;
        }

        async Task Func()
        {
            var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => rollbackRepository.RollbackNewPlayerSettingAsync(custId),
                "RollbackNewPlayerSettingAsync", processName, logger, true);

            if (isSuccess)
            {
                isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => rollbackRepository.RollbackNewPlayerUserInfoAsync(custId),
                    "RollbackNewPlayerUserInfoAsync", processName, logger, true);
            }

            if (isSuccess)
            {
                await ProcessUtils.ExecuteWithTryCatchAsync(() => rollbackRepository.RollbackNewPlayerAsync(custId),
                    "RollbackNewPlayerAsync", processName, logger, true);
            }
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void RollbackInfoNewPlayer(string processName, int custId)
    {
        if (!IsValidCustId(custId))
        {
            return;
        }

        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => rollbackRepository.RollbackNewPlayerAsync(custId),
                "RollbackNewPlayerAsync", processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void InsertNewPlayerAffiliateInfo(string processName, NewPlayerAffiliateParam param)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => affiliateRepository.InsertPlayerAffiliateAsync(param),
                "InsertPlayerAffiliateAsync", processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void ReinitBalanceReactivatePlayer(string processName, InitializeReactivatePlayerBalanceParam param)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(async () =>
            {
                var result = await balanceRepository.InitializeReactivatePlayerBalanceAsync(param);
                if (result?.Result is not null)
                {
                    await logDbRepository.WriteLogTrackingDBLOGAsync(result.Result);
                }
            }, "InitializeReactivatePlayerBalanceAsync", processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    private void DoWork(ServerDbConfig serverDbConfig, string processName, Func<Task> func)
    {
        using (ExecutionContext.SuppressFlow())
        {
            Task.Run(async () =>
            {
                try
                {
                    using var scope = serviceScopeFactory.CreateScope();
                    var contextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                    contextAccessor.HttpContext = new DefaultHttpContext();
                    ServerDbConfigUtils.SetServerDbConfigToHttpContext(serverDbConfig, contextAccessor);

                    await func();
                }
                catch (Exception exception)
                {
                    var messageBuilder = new StringBuilder();
                    messageBuilder.AppendLine($"ERROR [Background Task]: An error occurred in the {processName} background process.");
                    messageBuilder.AppendLine(ErrorMessageUtils.GenerateMessage(exception));
                    await logger.ErrorAsync(messageBuilder.ToString());
                }
            });
        }
    }

    private static bool IsValidCustId(int custId) => custId > 0;
}