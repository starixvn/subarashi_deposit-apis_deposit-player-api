namespace Fanex.DepositPlayerAPI.Application.Chains.MapSabaId;

using System.Text;
using Constants;
using Domain.Errors;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Services;
using Utils;

public class MapSabaIdGetServerDbConfigChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IHttpContextAccessor httpContextAccessor,
    IServerDbConfigRepository serverDbConfigRepository)
    : ChainBaseNonResultHandler<MapSabaIdChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => nameof(MapSabaIdGetServerDbConfigChainHandler);
    public override string FunctionName => FunctionNames.MapSabaId;

    public override async Task<ServiceResult> HandleInternal(MapSabaIdChainParam request)
    {
        var result = await serverDbConfigRepository.GetServerDbConfigByAgentIdAsync(request.CustId);
        if (result is null)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"[{FunctionName}] - DB server configuration not found for CustId={request.CustId}.");
            stringBuilder.AppendLine($"Request Info: {JsonConvert.SerializeObject(request, Formatting.Indented)}");

            return ServiceResult.Failure(ServiceErrors.SystemError, stringBuilder.ToString());
        }

        ServerDbConfigUtils.SetServerDbConfigToHttpContext(result, httpContextAccessor);

        return ServiceResult.Success();
    }
}