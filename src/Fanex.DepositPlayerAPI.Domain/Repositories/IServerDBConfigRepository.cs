namespace Fanex.DepositPlayerAPI.Domain.Repositories;

using Entities;

public interface IServerDbConfigRepository
{
    Task<List<ServerDbConfig>> GetAllServerDbConfigAsync();

    Task<ServerDbConfig> GetServerDbConfigByAgentIdAsync(int agentId);
}