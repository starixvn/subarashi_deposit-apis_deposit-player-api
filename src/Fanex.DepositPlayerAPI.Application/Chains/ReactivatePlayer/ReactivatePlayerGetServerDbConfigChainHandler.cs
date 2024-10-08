namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

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

public class ReactivatePlayerGetServerDbConfigChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IHttpContextAccessor httpContextAccessor,
    IServerDbConfigRepository serverDbConfigRepository)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[ReactivatePlayer] - ReactivatePlayerGetDBServerConfigChainHandler";

    public override async Task<ServiceResult> HandleInternal(ReactivatePlayerChainParam request)
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

        var result = allServerDbConfig.TryGetValue(request.Recommend, out var dbConfig)
            ? dbConfig
            : await serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(request.Recommend);

        if (result is null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"[ReactivatePlayer] - DB server configuration not found for AgentId={request.Recommend}.");
            stringBuilder.AppendLine($"Request Info: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

            return ServiceResult.Failure(PlayerError.CreateCustomerFailed, stringBuilder.ToString());
        }

        ServerDbConfigUtils.SetServerDbConfigToHttpContext(result, httpContextAccessor);
        UpdateRequest(result, request);

        return ServiceResult.Success();
    }

    private static void UpdateRequest(ServerDbConfig config, ReactivatePlayerChainParam request)
    {
        request.Site = config.Site;
    }
}