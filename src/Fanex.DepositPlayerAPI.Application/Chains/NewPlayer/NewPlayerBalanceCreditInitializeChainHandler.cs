namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Constants;
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

    public override string ChainName => nameof(NewPlayerBalanceCreditInitializeChainHandler);

    public override string FunctionName => FunctionNames.NewPlayer;

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var result = await balanceRepository.InitializeNewPlayerCreditBalanceAsync(request.CustId);

        return result.Match(
            extraInfo => ServiceResult<Player>.Success(null, extraInfo),
            ServiceResult<Player>.Failure);
    }

    public override void HandleError(NewPlayerChainParam request)
    {
        LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.RollbackFullNewPlayer)}");

        backgroundTask.RollbackFullNewPlayer(ChainName, request.CustId);
    }
}