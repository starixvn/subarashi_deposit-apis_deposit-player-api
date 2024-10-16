namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Domain.Entities;
using Domain.Extensions;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class NewPlayerInsertDefaultCommChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    ICommissionRepository commissionRepository)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => false;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerInsertDefaultCommChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        if (request.GroupDiscountId.HasValue && request.GroupDiscountId.Value != 0)
        {
            var param = new InsertDefaultCommissionParam
            {
                Site = request.Site,
                CustId = request.CustId,
                GroupDiscountId = request.GroupDiscountId
            };

            var result = await commissionRepository.InsertDefaultCommAsync(param);

            return result.Match(
                extraInfo => ServiceResult<Player>.Success(null, extraInfo),
                ServiceResult<Player>.Failure);
        }

        return ServiceResult<Player>.Success(null);
    }
}