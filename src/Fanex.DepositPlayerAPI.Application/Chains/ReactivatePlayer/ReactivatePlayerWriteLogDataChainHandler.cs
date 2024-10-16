namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

using Domain.Entities;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class ReactivatePlayerWriteLogDataChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    ILogDbRepository logDbRepository)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => false;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[ReactivatePlayer] - ReactivatePlayerWriteLogDataChainHandler";

    public override async Task<ServiceResult> HandleInternal(ReactivatePlayerChainParam request)
    {
        var initLogData = new LogTracking
        {
            CustId = request.CustId,
            Action = "REACTIVATION",
            Status = "OnProgress",
            Message = "Start reactivating customer"
        };

        await logDbRepository.WriteLogTrackingDBLOGAsync(initLogData);

        return ServiceResult.Success();
    }
}