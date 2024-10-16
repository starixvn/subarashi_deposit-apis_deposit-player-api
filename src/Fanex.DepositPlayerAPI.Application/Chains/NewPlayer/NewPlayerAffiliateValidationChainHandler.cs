namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Domain.Entities;
using Domain.Extensions;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class NewPlayerAffiliateValidationChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IAffiliateRepository affiliateRepository)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerAffiliateValidationChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        if (request.AffiliateId is > 0)
        {
            var result = await affiliateRepository.ValidateAffiliateAsync(request.AffiliateId.Value);

            return result.Match(
                extraInfo => ServiceResult<Player>.Success(null, extraInfo),
                ServiceResult<Player>.Failure);
        }

        return ServiceResult<Player>.Success(null);
    }
}