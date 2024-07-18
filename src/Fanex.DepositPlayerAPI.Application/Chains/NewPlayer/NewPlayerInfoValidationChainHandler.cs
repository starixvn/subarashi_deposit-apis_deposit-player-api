namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Common.Interfaces;
using Domain.Entities;
using Domain.Extensions;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;

public class NewPlayerInfoValidationChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerInfoValidationChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var param = new ValidatePlayerInfoParam
        {
            DepositUserName = request.DepositUserName,
            AgentId = request.AgentId
        };

        var result = await playerRepository.ValidatePlayerInfoAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            extraInfo => ServiceResult<Player>.Success(null, extraInfo),
            ServiceResult<Player>.Failure);
    }

    private static void UpdateRequest(ValidatePlayerInfoOutputParam output, NewPlayerChainParam request)
    {
        request.CurrencyName = output.CurrencyName;
        request.Site = output.Site;
        request.Username2 = output.Username2;
        request.Recommend = output.Recommend;
    }
}