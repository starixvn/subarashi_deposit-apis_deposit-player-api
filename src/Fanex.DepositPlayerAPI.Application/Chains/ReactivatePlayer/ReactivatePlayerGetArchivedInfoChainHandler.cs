namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

using Common.Interfaces;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;

public class ReactivatePlayerGetArchivedInfoChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[ReactivatePlayer] - ReactivatePlayerGetArchivedInfoChainHandler";

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
        LogAggregate.AppendLog("[ReactivatePlayer] - Run Error Handler Task: WriteLogTracking");

        if (request?.LogTracking is not null)
        {
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