namespace Fanex.DepositPlayerAPI.Application.Handlers;

using Chains;
using Commands;
using Domain.Results;
using MediatR;
using Services;
using Utils;

public class MapSabaIdHandler(
    ILogAggregate logAggregate,
    IChainHandlerFactory chainHandlerFactory) : IRequestHandler<MapSabaIdCommand, ServiceResult>
{
    public async Task<ServiceResult> Handle(MapSabaIdCommand command, CancellationToken cancellationToken)
    {
        logAggregate.AppendLog("Started processing...");

        var rootChainHandler = chainHandlerFactory.CreateMapSabaIdChainParam();
        var param = ParamMapper.BuildMapSabaIdChainParam(command);

        var result = await rootChainHandler.Handle(param);

        logAggregate.AppendLog("Finished processing...");

        return result;
    }
}