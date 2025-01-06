namespace Fanex.DepositPlayerAPI.Application.Chains.MapSabaId;

using Constants;
using Domain.Entities;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class MapSabaIdInsertDataChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    ISabaRepository sabaRepository,
    IBackgroundTask backgroundTask)
    : ChainBaseNonResultHandler<MapSabaIdChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => true;
    public override string ChainName => nameof(MapSabaIdInsertDataChainHandler);
    public override string FunctionName => FunctionNames.MapSabaId;

    public override async Task<ServiceResult> HandleInternal(MapSabaIdChainParam request)
    {
        var param = new MapSabaInfoParam
        {
            CustId = request.CustId,
            CustType = request.CustType,
            SabaId = request.SabaId,
            IsTest = request.IsTest
        };

        var result = await sabaRepository.MapSabaInfoAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result;
    }

    public override void HandleSuccess(MapSabaIdChainParam request)
    {
        LogAggregate.AppendLog($"Background Task: {nameof(backgroundTask.WriteLogData)}");

        backgroundTask.WriteLogData(ChainName, request.LogData);
    }

    private static void UpdateRequest(LogData logData, MapSabaIdChainParam request)
    {
        request.LogData = logData;
    }
}