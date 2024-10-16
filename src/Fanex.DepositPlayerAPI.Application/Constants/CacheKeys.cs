namespace Fanex.DepositPlayerAPI.Application.Constants;

public static class CacheKeys
{
    public static string ServerDbConfigAll => "ServerDbConfigAll";

    public static Func<int, string> ServerDbConfigByAgentId => agentId => $"ServerDbConfigByAgentId_{agentId}";
}