namespace Fanex.DepositPlayerAPI.Application.Chains.ReactivatePlayer;

using Constants;
using Domain.Params;
using Domain.Params.Chains;
using Domain.Repositories;
using Domain.Results;
using Logging;
using Services;

public class ReactivatePlayerInsertDefaultGroupCommChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    ICommissionRepository commissionRepository)
    : ChainBaseNonResultHandler<ReactivatePlayerChainParam>(logger, logAggregate)
{
    public override bool IsBreakOnError => false;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => nameof(ReactivatePlayerInsertDefaultGroupCommChainHandler);

    public override string FunctionName => FunctionNames.ReactivatePlayer;

    public override async Task<ServiceResult> HandleInternal(ReactivatePlayerChainParam request)
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

        return result;
    }

    private static void UpdateRequest(InsertDefaultGroupCommissionOutputParam output, ReactivatePlayerChainParam request)
    {
        request.DefaultCommissions = output.DefaultCommissions;
        request.GroupDiscountId = output.GroupDiscountId;
    }
}