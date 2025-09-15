namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

using Constants;
using Domain.Entities;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class ReactivateBalanceInitializeChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IBalanceRepository balanceRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => false;

    public override bool IsBreakOnSuccess => true;

    public override string ChainName => nameof(ReactivateBalanceInitializeChainHandler);

    public override string FunctionName => FunctionNames.ReactivatePlayer;

    public override async Task<ServiceResult> HandleInternal(ReactivatePlayerChainParam request)
    {
        var reinitBalanceParam = new InitializeReactivatePlayerBalanceParam
        {
            CustId = request.CustId,
            CustIdBalanceDummy = request.CustIdBalanceDummy,
            ClosingBalance = request.ClosingBalance
        };

        var result = await balanceRepository.InitializeReactivatePlayerBalanceAsync(reinitBalanceParam);
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

    private static void UpdateRequest(LogTracking logTracking, ReactivatePlayerChainParam request)
    {
        request.LogTracking = logTracking;
    }
}