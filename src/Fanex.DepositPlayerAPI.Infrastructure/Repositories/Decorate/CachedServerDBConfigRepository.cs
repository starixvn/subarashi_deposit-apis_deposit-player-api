namespace Fanex.DepositPlayerAPI.Infrastructure.Repositories.Decorate;

using Constants;
using Domain.Entities;
using Domain.Repositories;
using Extensions;
using Microsoft.Extensions.Caching.Memory;

public class CachedServerDbConfigRepository(
    IServerDbConfigRepository serverDbConfigRepository,
    IMemoryCache cache)
    : IServerDbConfigRepository
{
    public async Task<List<ServerDbConfig>> GetAllServerDbConfigAsync()
    {
        var result = await cache.GetOrCreateFromCacheWithoutExpired(
            CacheKeys.ServerDbConfigAll,
            serverDbConfigRepository.GetAllServerDbConfigAsync);

        return result;
    }

    public async Task<ServerDbConfig> GetServerDbConfigByAgentIdAsync(int agentId)
    {
        var result = await cache.GetOrCreateFromCacheWithoutExpired(
            CacheKeys.ServerDbConfigByAgentId(agentId),
            () => serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(agentId));

        return result;
    }
}