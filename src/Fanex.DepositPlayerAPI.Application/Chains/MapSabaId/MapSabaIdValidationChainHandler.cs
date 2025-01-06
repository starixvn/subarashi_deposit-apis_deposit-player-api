namespace Fanex.DepositPlayerAPI.Application.Chains.MapSabaId;

using Constants;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class MapSabaIdValidationChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    ISabaRepository sabaRepository)
    : ChainBaseNonResultHandler<MapSabaIdChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;
    public override bool IsBreakOnSuccess => false;
    public override string ChainName => nameof(MapSabaIdValidationChainHandler);
    public override string FunctionName => FunctionNames.MapSabaId;

    public override async Task<ServiceResult> HandleInternal(MapSabaIdChainParam request)
    {
        var param = new ValidateSabaInfoParam
        {
            CustId = request.CustId,
            CustType = request.CustType,
            SabaId = request.SabaId
        };

        var result = await sabaRepository.ValidateSabaInfoAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result;
    }

    private static void UpdateRequest(ValidateSabaInfoOutputParam output, MapSabaIdChainParam request)
    {
        request.IsTest = output.IsTest;
    }
}