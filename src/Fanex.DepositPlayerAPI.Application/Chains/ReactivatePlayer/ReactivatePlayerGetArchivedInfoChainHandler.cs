namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

using Constants;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class ReactivatePlayerGetArchivedInfoChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => nameof(ReactivatePlayerGetArchivedInfoChainHandler);

    public override string FunctionName => FunctionNames.ReactivatePlayer;

    public override async Task<ServiceResult> HandleInternal(ReactivatePlayerChainParam request)
    {
        var result = await playerRepository.GetArchivedInfoAsync(request.CustId);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result;
    }

    public override void HandleError(ReactivatePlayerChainParam request)
    {
        if (request?.LogTracking is not null)
        {
            LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.WriteLogTracking)}");

            backgroundTask.WriteLogTracking(ChainName, request.LogTracking);
        }
    }

    private static void UpdateRequest(GetArchivedPlayerInfoOutputParam output, ReactivatePlayerChainParam request)
    {
        request.CustomerInfo = output.CustomerInfo;
        request.LogTracking = output.LogTracking;
        request.Recommend = output.Recommend;
    }
}