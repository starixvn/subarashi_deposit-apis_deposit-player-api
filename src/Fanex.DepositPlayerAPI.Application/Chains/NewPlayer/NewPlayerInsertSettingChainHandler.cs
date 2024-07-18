namespace Fanex.DepositPlayerAPI.Application.Chains.NewPlayer;

using Common.Interfaces;
using Domain.Entities;
using Domain.Extensions;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;

public class NewPlayerInsertSettingChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseHandler<NewPlayerChainParam, Player>(logger, logAggregate)
{
    private readonly List<int> _excludeErrorCode = [950, 952, 953];

    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => "[NewPlayer] - NewPlayerInsertSettingChainHandler";

    public override async Task<ServiceResult<Player>> HandleInternal(NewPlayerChainParam request)
    {
        var param = new InsertNewPlayerSettingParam
        {
            CustId = request.CustId,
            Username = request.Username,
            Username2 = request.Username2,
            DepositUserId = request.DepositUserId,
            Recommend = request.Recommend,
            Password = request.Password,
            OddsType = request.OddsType,
            Country = request.Country,
            SubsidiaryId = request.SubsidiaryId,
            AffiliateId = request.AffiliateId,
            DefaultCommissions = request.DefaultCommissions,
            GroupDiscountId = request.GroupDiscountId,
            Site = request.Site
        };

        var result = await playerRepository.InsertNewPlayerSettingAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            extraInfo => ServiceResult<Player>.Success(null, extraInfo),
            ServiceResult<Player>.Failure);
    }

    public override bool IsProcessFailure(ServiceResult<Player> response)
    {
        return base.IsProcessFailure(response) && !_excludeErrorCode.Contains(response.Error.Code);
    }

    public override void HandleError(NewPlayerChainParam request)
    {
        LogAggregate.AppendLog("[NewPlayer] - Run Error Handler Task: RollbackFullNewPlayer");

        backgroundTask.RollbackFullNewPlayer(ChainName, request.CustId);
    }

    public override void HandleSuccess(NewPlayerChainParam request)
    {
        LogAggregate.AppendLog("[NewPlayer] - Run Success Handler Task: WriteLogData, InsertNewPlayerAffiliateInfo");

        backgroundTask.WriteLogData(ChainName, request.LogData);

        if (request.AffiliateId is > 0)
        {
            var param = new NewPlayerAffiliateParam
            {
                CustId = request.CustId,
                AffiliateId = request.AffiliateId.Value,
                Username2 = request.Username2,
                Username = request.Username,
                DepositUserId = request.DepositUserId,
                Currency = request.Currency,
                Recommend = request.Recommend
            };

            backgroundTask.InsertNewPlayerAffiliateInfo(ChainName, param);
        }
    }

    private static void UpdateRequest(InsertNewPlayerSettingOutputParam output, NewPlayerChainParam request)
    {
        request.Currency = output.Currency;
        request.LogData = output.LogData;
    }
}