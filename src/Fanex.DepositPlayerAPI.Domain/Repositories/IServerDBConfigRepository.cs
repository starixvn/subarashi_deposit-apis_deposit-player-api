namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;

public interface IServerDbConfigRepository
{
    Task<Dictionary<int, ServerDbConfig>> GetAllServerDbConfigAsync();

    Task<ServerDbConfig> GetServerDbConfigByAgentIdAsync(int agentId);
}