namespace Fanex.DepositPlayerAPI.Application.Chains.NewShadowAccount;

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

public class NewShadowAccountGetServerDbConfigChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IServerDbConfigRepository serverDbConfigRepository,
    IHttpContextAccessor httpContextAccessor,
    ICacheProvider cacheProvider)
    : ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => "[NewShadowAccount] - NewShadowAccountGetServerDbConfigChainHandler";

    public override async Task<ServiceResult<ShadowAccount>> HandleInternal(NewShadowAccountChainParam request)
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

            stringBuilder.AppendLine($"[NewShadowAccount] - DB server configuration not found for AgentId={request.AgentId}.");
            stringBuilder.AppendLine($"Request Info: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

            return ServiceResult<ShadowAccount>.Failure(PlayerError.CreateCustomerFailed, stringBuilder.ToString());
        }

        ServerDbConfigUtils.SetServerDbConfigToHttpContext(result, httpContextAccessor);
        UpdateRequest(result, request);

        return ServiceResult<ShadowAccount>.Success(null);
    }

    private static void UpdateRequest(ServerDbConfig config, NewShadowAccountChainParam request)
    {
        request.SiteDbGroupId = config.CustDbGroupId;
    }
}