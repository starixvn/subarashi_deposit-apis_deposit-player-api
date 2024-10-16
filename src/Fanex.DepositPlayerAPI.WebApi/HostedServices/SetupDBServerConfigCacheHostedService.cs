namespace Fanex.DepositPlayerAPI.WebApi.HostedServices;

using System.Text;
using Application.Constants;
using Application.Services;
using Application.Utils;
using Domain.Entities;
using Domain.Repositories;
using Logging;
using Microsoft.IdentityModel.Tokens;

public class SetupDbServerConfigCacheHostedService(IServiceProvider services) : IHostedService
{
    private const int MaxAttempt = 3;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();
        var serverDbConfigRepository = scope.ServiceProvider.GetRequiredService<IServerDbConfigRepository>();
        var cacheProvider = scope.ServiceProvider.GetRequiredService<ICacheProvider>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        var attempt = 0;
        var data = new Dictionary<int, ServerDbConfig>();
        var extraInfo = new StringBuilder();
        extraInfo.AppendLine($"{dateTimeProvider.UtcNow:yyyy-MM-dd HH:mm:ss.fff}: Start setup cache!");
        while (data.IsNullOrEmpty() && ++attempt <= MaxAttempt)
        {
            try
            {
                data = await cacheProvider.GetOrCreateFromCacheWithoutExpiredAsync(
                    CacheKeys.ServerDbConfigAll,
                    serverDbConfigRepository.GetAllServerDbConfigAsync);

                extraInfo.AppendLine($"\n{dateTimeProvider.UtcNow:yyyy-MM-dd HH:mm:ss.fff}: Cache setup successful on attempt {attempt}!\n");
            }
            catch (Exception exception)
            {
                extraInfo.AppendLine($"\n{dateTimeProvider.UtcNow:yyyy-MM-dd HH:mm:ss.fff}: Cache setup failed on attempt {attempt}!\n");
                extraInfo.AppendLine(ErrorMessageUtils.GenerateMessage(exception));
            }
        }

        extraInfo.AppendLine($"{dateTimeProvider.UtcNow:yyyy-MM-dd HH:mm:ss.fff}: Finish setup cache!");

        if (data.IsNullOrEmpty())
        {
            await logger.CriticalAsync($"ERROR: Cache setup failed after {MaxAttempt} attempts!\nEXTRA INFO:\n{extraInfo}");
            return;
        }

        await logger.InfoAsync($"SUCCESS: Cache setup successful at {attempt} attempts! Total rows of data: {data.Count}.\nEXTRA INFO:\n{extraInfo}");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}