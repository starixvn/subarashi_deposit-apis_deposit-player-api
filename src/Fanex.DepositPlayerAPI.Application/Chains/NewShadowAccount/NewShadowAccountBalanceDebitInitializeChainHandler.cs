namespace Fanex.DepositPlayerAPI.Application.Chains.NewShadowAccount;

using Constants;
using Domain.Entities;
using Domain.Extensions;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class NewShadowAccountBalanceDebitInitializeChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IBalanceRepository balanceRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>(logger, logAggregate)

{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => true;

    public override string ChainName => nameof(NewShadowAccountBalanceDebitInitializeChainHandler);

    public override string FunctionName => FunctionNames.NewShadowAccount;

    public override async Task<ServiceResult<ShadowAccount>> HandleInternal(NewShadowAccountChainParam request)
    {
        var result = await balanceRepository.InitializeNewPlayerDebitBalanceAsync(request.CustId);

        return result.Match(
            extraInfo =>
            {
                var data = new ShadowAccount
                {
                    CustId = request.CustId,
                    Username = request.Username
                };
                return ServiceResult<ShadowAccount>.Success(data, extraInfo);
            },
            ServiceResult<ShadowAccount>.Failure);
    }

    public override void HandleError(NewShadowAccountChainParam request)
    {
        LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.RollbackFullNewPlayer)}");

        backgroundTask.RollbackFullNewPlayer(ChainName, request.CustId);
    }
}