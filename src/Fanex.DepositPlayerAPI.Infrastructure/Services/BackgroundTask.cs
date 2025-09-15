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

public record BackgroundTaskDependencies(
    IRollbackRepository RollbackRepository,
    ILogDbRepository LogDbRepository,
    IAffiliateRepository AffiliateRepository,
    ICommissionRepository CommissionRepository,
    IBalanceRepository BalanceRepository);

public class BackgroundTask(
    BackgroundTaskDependencies dependencies,
    ILogger logger,
    IServiceScopeFactory serviceScopeFactory,
    IHttpContextAccessor httpContextAccessor) : IBackgroundTask
{
    private ServerDbConfig ServerDbConfig => ServerDbConfigUtils.GetServerDbConfigFromHttpContext(httpContextAccessor);

    public void WriteLogData(string processName, LogData logData)
    {
        async Task Func()
        {
            var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.LogDbRepository.WriteLogDataDBLOGAsync(logData),
                nameof(dependencies.LogDbRepository.WriteLogDataDBLOGAsync), processName, logger, true);

            if (!isSuccess)
            {
                await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.LogDbRepository.WriteLogDataDBCUSTAsync(logData),
                    nameof(dependencies.LogDbRepository.WriteLogDataDBCUSTAsync), processName, logger, true);
            }
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void WriteLogTracking(string processName, LogTracking logTracking)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.LogDbRepository.WriteLogTrackingDBLOGAsync(logTracking),
                nameof(dependencies.LogDbRepository.WriteLogTrackingDBLOGAsync), processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void InsertDefaultCommLicenseeDb(string processName, InsertDefaultCommissionParam param)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.CommissionRepository.InsertDefaultCommAsync(param),
                nameof(dependencies.CommissionRepository.InsertDefaultCommAsync), processName, logger, true);
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
            var isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.RollbackRepository.RollbackNewPlayerSettingAsync(custId),
                nameof(dependencies.RollbackRepository.RollbackNewPlayerSettingAsync), processName, logger, true);

            if (isSuccess)
            {
                isSuccess = await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.RollbackRepository.RollbackNewPlayerUserInfoAsync(custId),
                    nameof(dependencies.RollbackRepository.RollbackNewPlayerUserInfoAsync), processName, logger, true);
            }

            if (isSuccess)
            {
                await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.RollbackRepository.RollbackNewPlayerAsync(custId),
                    nameof(dependencies.RollbackRepository.RollbackNewPlayerAsync), processName, logger, true);
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
            await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.RollbackRepository.RollbackNewPlayerAsync(custId),
                nameof(dependencies.RollbackRepository.RollbackNewPlayerAsync), processName, logger, true);
        }

        DoWork(ServerDbConfig, processName, Func);
    }

    public void InsertNewPlayerAffiliateInfo(string processName, NewPlayerAffiliateParam param)
    {
        async Task Func()
        {
            await ProcessUtils.ExecuteWithTryCatchAsync(() => dependencies.AffiliateRepository.InsertPlayerAffiliateAsync(param),
                nameof(dependencies.AffiliateRepository.InsertPlayerAffiliateAsync), processName, logger, true);
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
                    messageBuilder.AppendLine($"ERROR [Background Task]: An exception occurred in the {processName} background process.");
                    messageBuilder.AppendLine(ErrorMessageUtils.GenerateMessage(exception));
                    await logger.ErrorAsync(messageBuilder.ToString());
                }
            });
        }
    }

    private static bool IsValidCustId(int custId) => custId > 0;
}