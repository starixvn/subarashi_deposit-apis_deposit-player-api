namespace Fanex.DepositPlayerAPI.WebApi.HealthChecks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class MemoryHealthCheck(IMemoryCache memoryCache) : IHealthCheck
{
    private readonly long Threshold = 1024L;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var allocated = GC.GetTotalMemory(false) / (1024L * 1024L);
        var usedMemory = new
        {
            AllocatedMBs = allocated,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2)
        };

        var data = new Dictionary<string, object>
        {
            { "Memory Cache", memoryCache.GetCurrentStatistics() },
            { "Memory Used", usedMemory }
        };

        var result = new HealthCheckResult(
            allocated < Threshold ? HealthStatus.Healthy : HealthStatus.Degraded,
            $"Reports degraded status if allocated bytes >= {Threshold} mbs.",
            null,
            data);

        return Task.FromResult(result);
    }
}