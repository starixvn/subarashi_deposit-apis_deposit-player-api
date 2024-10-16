namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Domain.Entities;
using Domain.Extensions;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class NewPlayerBalanceCreditInitializeChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IBalanceRepository balanceRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerBalanceCreditInitializeChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var result = await balanceRepository.InitializeNewPlayerCreditBalanceAsync(request.CustId);

        return result.Match(
            extraInfo => ServiceResult<Player>.Success(null, extraInfo),
            ServiceResult<Player>.Failure);
    }

    public override void HandleError(NewPlayerChainParam request)
    {
        LogAggregate.AppendLog(
            "[NewPlayer] - Run Error Handler Task: RollbackFullNewPlayer");

        backgroundTask.RollbackFullNewPlayer(ChainName, request.CustId);
    }
}