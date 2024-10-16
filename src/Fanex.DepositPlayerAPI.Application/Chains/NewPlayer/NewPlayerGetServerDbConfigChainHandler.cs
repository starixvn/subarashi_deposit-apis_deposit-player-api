namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using System.Text;
using Constants;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Services;
using Utils;

public class NewPlayerGetServerDbConfigChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IServerDbConfigRepository serverDbConfigRepository,
    IHttpContextAccessor httpContextAccessor,
    ICacheProvider cacheProvider)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => "[NewPlayer] - NewPlayerGetServerDbConfigChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var allServerDbConfig = cacheProvider.GetValueFromCache(CacheKeys.ServerDbConfigAll, new Dictionary<int, ServerDbConfig>());

        var result = allServerDbConfig.TryGetValue(request.AgentId, out var dbConfig)
            ? dbConfig
            : await cacheProvider.GetOrCreateFromCacheWithoutExpiredAsync(
                CacheKeys.ServerDbConfigByAgentId(request.AgentId),
                () => serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(request.AgentId));

        if (result is null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"[NewPlayer] - DB server configuration not found for AgentId={request.AgentId}.");
            stringBuilder.AppendLine($"Request Info: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

            return ServiceResult<Player>.Failure(PlayerError.CreateCustomerFailed, stringBuilder.ToString());
        }

        ServerDbConfigUtils.SetServerDbConfigToHttpContext(result, httpContextAccessor);
        UpdateRequest(result, request);

        return ServiceResult<Player>.Success(null);
    }

    private static void UpdateRequest(ServerDbConfig config, NewPlayerChainParam request)
    {
        request.SiteDbGroupId = config.CustDbGroupId;
    }
}