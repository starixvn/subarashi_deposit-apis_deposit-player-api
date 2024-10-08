namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using System.Text;
using Common.Interfaces;
using Domain.Entities;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Utils;

public class NewPlayerGetServerDbConfigChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IServerDbConfigRepository serverDbConfigRepository,
    IHttpContextAccessor httpContextAccessor)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => "[NewPlayer] - NewPlayerGetServerDbConfigChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var allServerDbConfig = new Dictionary<int, ServerDbConfig>();
        try
        {
            allServerDbConfig = await serverDbConfigRepository.GetAllServerDbConfigAsync();
        }
        catch (Exception ex)
        {
            await Logger.ErrorAsync(ErrorMessageUtils.GenerateMessage(ex));
        }

        var result = allServerDbConfig.TryGetValue(request.AgentId, out var dbConfig)
            ? dbConfig
            : await serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(request.AgentId);

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