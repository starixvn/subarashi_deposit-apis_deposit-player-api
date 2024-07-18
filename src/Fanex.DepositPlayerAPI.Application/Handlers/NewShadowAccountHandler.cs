namespace Fanex.DepositPlayerAPI.Application.Handlers;

using Chains;
using Commands;
using Common.Interfaces;
using Domain.Entities;
using Domain.Results;
using MediatR;
using Utils;

public class NewShadowAccountHandler(
    IChainHandlerFactory chainHandlerFactory,
    ILogAggregate logAggregate) : IRequestHandler<NewShadowAccountCommand, ServiceResult<ShadowAccount>>
{
    public async Task<ServiceResult<ShadowAccount>> Handle(
        NewShadowAccountCommand command,
        CancellationToken cancellationToken)
    {
        logAggregate.AppendLog("Started processing...");

        var rootChainHandler = chainHandlerFactory.CreateNewShadowAccountChainHandler();
        var param = ParamMapper.MapNewShadowAccountChainParam(command);
        var result = await rootChainHandler.Handle(param);

        logAggregate.AppendLog("Finished processing...");

        return result;
    }
}