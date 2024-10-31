namespace Fanex.DepositPlayerAPI.Application.Chains.NewShadowAccount;

using Constants;
using Domain.Entities;
using Domain.Extensions;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class NewShadowAccountInsertSettingChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => nameof(NewShadowAccountInsertSettingChainHandler);

    public override string FunctionName => FunctionNames.NewShadowAccount;

    public override async Task<ServiceResult<ShadowAccount>> HandleInternal(NewShadowAccountChainParam request)
    {
        var param = new InsertShadowAccountSettingParam
        {
            CustId = request.CustId,
            Username = request.Username,
            Username2 = request.Username2,
            RealCustId = request.RealCustId,
            AgentId = request.AgentId
        };

        var result = await playerRepository.InsertShadowAccountSettingAsync(param);

        if (result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            extraInfo => ServiceResult<ShadowAccount>.Success(null, extraInfo),
            ServiceResult<ShadowAccount>.Failure);
    }

    public override void HandleError(NewShadowAccountChainParam request)
    {
        LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.RollbackFullNewPlayer)}");

        backgroundTask.RollbackFullNewPlayer(ChainName, request.CustId);
    }

    public override void HandleSuccess(NewShadowAccountChainParam request)
    {
        LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.WriteLogData)}");

        backgroundTask.WriteLogData(ChainName, request.LogData);
    }

    private static void UpdateRequest(LogData logData, NewShadowAccountChainParam request)
    {
        request.LogData = logData;
    }
}