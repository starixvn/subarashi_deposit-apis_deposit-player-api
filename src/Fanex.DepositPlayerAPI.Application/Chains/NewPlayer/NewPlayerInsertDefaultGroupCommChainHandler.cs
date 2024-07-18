namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Common.Interfaces;
using Domain.Entities;
using Domain.Extensions;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;

public class NewPlayerInsertDefaultGroupCommChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    ICommissionRepository commissionRepository)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => false;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerInsertDefaultGroupCommChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var commProducts = await commissionRepository.GetPlayerCommProductsAsync();

        var param = new InsertDefaultGroupCommissionParam
        {
            Site = request.Site,
            CommissionProducts = commProducts
        };

        var result = await commissionRepository.InsertDefaultGroupCommissionAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            extraInfo => ServiceResult<Player>.Success(null, extraInfo),
            ServiceResult<Player>.Failure);
    }

    private static void UpdateRequest(InsertDefaultGroupCommissionOutputParam output, NewPlayerChainParam request)
    {
        request.DefaultCommissions = output.DefaultCommissions;
        request.GroupDiscountId = output.GroupDiscountId;
    }
}