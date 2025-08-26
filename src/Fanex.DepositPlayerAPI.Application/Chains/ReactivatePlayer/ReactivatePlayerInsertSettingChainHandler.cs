namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

using Constants;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class ReactivatePlayerInsertSettingChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    private readonly List<int> _excludeErrorCode = [950, 952, 953];

    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => nameof(ReactivatePlayerInsertSettingChainHandler);

    public override string FunctionName => FunctionNames.ReactivatePlayer;

    public override bool IsProcessFailure(ServiceResult response)
    {
        return base.IsProcessFailure(response)
               && !_excludeErrorCode.Contains(response.Error.Code);
    }

    public override async Task<ServiceResult> HandleInternal(ReactivatePlayerChainParam request)
    {
        var param = new InsertReactivePlayerSettingParam
        {
            CustId = request.CustId,
            CustomerInfo = request.CustomerInfo,
            SubsidiaryId = request.SubsidiaryId,
            DefaultCommissions = request.DefaultCommissions,
            GroupDiscountId = request.GroupDiscountId
        };

        var result = await playerRepository.InsertReactivePlayerSettingAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result;
    }

    public override void HandleError(ReactivatePlayerChainParam request)
    {
        if (request?.LogTracking is not null)
        {
            LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.WriteLogTracking)}");

            backgroundTask.WriteLogTracking(ChainName, request.LogTracking);
        }
    }

    public override void HandleSuccess(ReactivatePlayerChainParam request)
    {
        LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.WriteLogData)}");

        backgroundTask.WriteLogData(ChainName, request.LogData);

        if (request?.LogTracking is not null)
        {
            LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.WriteLogTracking)}");

            backgroundTask.WriteLogTracking(ChainName, request.LogTracking);
        }

        if (request.GroupDiscountId.HasValue && request.GroupDiscountId.Value != 0)
        {
            var param = new InsertDefaultCommissionParam
            {
                Site = request.Site,
                CustId = request.CustId,
                GroupDiscountId = request.GroupDiscountId
            };

            LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.InsertDefaultCommLicenseeDb)}");

            backgroundTask.InsertDefaultCommLicenseeDb(ChainName, param);
        }
    }

    private static void UpdateRequest(InsertReactivePlayerSettingOutputParam output, ReactivatePlayerChainParam request)
    {
        request.CustIdBalanceDummy = output.CustIdBalanceDummy;
        request.ClosingBalance = output.ClosingBalance;
        request.LogTracking = output.LogTracking;
        request.LogData = output.LogData;
    }
}