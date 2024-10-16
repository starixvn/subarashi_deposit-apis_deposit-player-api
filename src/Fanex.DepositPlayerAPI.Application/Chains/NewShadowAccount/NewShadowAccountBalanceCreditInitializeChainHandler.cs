namespace Fanex.DepositPlayerAPI.Application.Chains.NewShadowAccount;

using Domain.Entities;
using Domain.Extensions;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class NewShadowAccountBalanceCreditInitializeChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IBalanceRepository balanceRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => "[NewShadowAccount] - NewShadowAccountBalanceCreditInitializeChainHandler";

    public override async Task<ServiceResult<ShadowAccount>> HandleInternal(NewShadowAccountChainParam request)
    {
        var result = await balanceRepository.InitializeNewPlayerCreditBalanceAsync(request.CustId);

        return result.Match(
            extraInfo => ServiceResult<ShadowAccount>.Success(null, extraInfo),
            ServiceResult<ShadowAccount>.Failure);
    }

    public override void HandleError(NewShadowAccountChainParam request)
    {
        LogAggregate.AppendLog(
            "[NewShadowAccount] - Run Error Handler Task: RollbackFullNewPlayer");

        backgroundTask.RollbackFullNewPlayer(ChainName, request.CustId);
    }
}