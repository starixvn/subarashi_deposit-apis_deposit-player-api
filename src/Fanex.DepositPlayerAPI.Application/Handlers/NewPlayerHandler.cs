namespace Fanex.DepositPlayerAPI.Application.Handlers;

using Chains;
using Commands;
using Domain.Entities;
using Domain.Results;
using MediatR;
using Services;
using Utils;

public class NewPlayerHandler(
    ILogAggregate logAggregate,
    IChainHandlerFactory chainHandlerFactory) : IRequestHandler<NewPlayerCommand, ServiceResult<Player>>
{
    public async Task<ServiceResult<Player>> Handle(NewPlayerCommand command, CancellationToken cancellationToken)
    {
        logAggregate.AppendLog("Started processing...");

        var rootChainHandler = chainHandlerFactory.CreateNewPlayerChainHandler();
        var param = ParamMapper.MapNewPlayerChainParam(command);
        var result = await rootChainHandler.Handle(param);

        logAggregate.AppendLog("Finished processing...");

        return result;
    }
}