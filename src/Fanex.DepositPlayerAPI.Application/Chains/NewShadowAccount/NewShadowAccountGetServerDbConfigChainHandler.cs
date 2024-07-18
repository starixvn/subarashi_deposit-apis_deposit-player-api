namespace Fanex.DepositPlayerAPI.Application.Chains.NewShadowAccount;

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

public class NewShadowAccountGetServerDbConfigChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IServerDbConfigRepository serverDbConfigRepository,
    IHttpContextAccessor httpContextAccessor)
    : ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => "[NewShadowAccount] - NewShadowAccountGetServerDbConfigChainHandler";

    public override async Task<ServiceResult<ShadowAccount>> HandleInternal(NewShadowAccountChainParam request)
    {
        var allServerDbConfig = new List<ServerDbConfig>();
        try
        {
            allServerDbConfig = await serverDbConfigRepository.GetAllServerDbConfigAsync();
        }
        catch (Exception ex)
        {
            await Logger.ErrorAsync(ErrorMessageUtils.GenerateMessage(ex));
        }

        var result = allServerDbConfig.Find(x => x.CustId == request.AgentId) ??
                     await serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(request.AgentId);

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