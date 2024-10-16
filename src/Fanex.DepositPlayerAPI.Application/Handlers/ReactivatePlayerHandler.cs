namespace Fanex.DepositPlayerAPI.Application.Handlers;

using Chains;
using Commands;
using Domain.Results;
using MediatR;
using Services;
using Utils;

public class ReactivatePlayerHandler(
    ILogAggregate logAggregate,
    IChainHandlerFactory chainHandlerFactory) : IRequestHandler<ReactivatePlayerCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(ReactivatePlayerCommand command, CancellationToken cancellationToken)
    {
        logAggregate.AppendLog("Started processing...");

        var rootChainHandler = chainHandlerFactory.CreateReactivatePlayerChainHandler();
        var param = ParamMapper.MapReactivatePlayerChainParam(command);

        var result = await rootChainHandler.Handle(param);

        logAggregate.AppendLog("Finished processing...");

        return result;
    }
}