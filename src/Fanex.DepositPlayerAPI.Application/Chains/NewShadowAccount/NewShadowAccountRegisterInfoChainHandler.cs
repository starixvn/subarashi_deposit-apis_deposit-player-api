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

public class NewShadowAccountRegisterInfoChainHandler(
    ILogger logger,
    ILogAggregate logAggregate,
    IPlayerRepository playerRepository)
    : ChainBaseHandler<NewShadowAccountChainParam, ShadowAccount>(logger, logAggregate)
{
    public override bool IsBreakOnError => true;

    public override bool IsBreakOnSuccess => false;

    public override string ChainName => nameof(NewShadowAccountRegisterInfoChainHandler);

    public override string FunctionName => FunctionNames.NewShadowAccount;

    public override async Task<ServiceResult<ShadowAccount>> HandleInternal(NewShadowAccountChainParam request)
    {
        var param = new InsertShadowAccountInfoParam
        {
            AgentId = request.AgentId,
            SiteDbGroupId = request.SiteDbGroupId,
            IsTest = request.IsTest
        };

        var result = await playerRepository.InsertShadowAccountInfoAsync(param);

        if (result?.Result is not null)
        {
            UpdateRequest(result.Result, request);
        }

        return result.Match(
            extraInfo => ServiceResult<ShadowAccount>.Success(null, extraInfo),
            ServiceResult<ShadowAccount>.Failure);
    }

    private static void UpdateRequest(InsertShadowAccountInfoOutputParam output, NewShadowAccountChainParam request)
    {
        request.Username = output.Username;
        request.Username2 = output.Username2;
        request.CustId = output.CustId;
    }
}