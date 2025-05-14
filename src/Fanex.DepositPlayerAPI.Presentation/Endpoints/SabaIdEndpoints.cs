namespace Fanex.DepositPlayerAPI.Presentation.Endpoints;

using System.Net;
using Application.Services;
using Domain.Extensions;
using Domain.Options;
using DTOs.Request;
using DTOs.Response;
using Extensions;
using Filters;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Utils;

public class SabaIdEndpoints : IEndpoints
{
    public string GroupName => "SabaIds";

    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this);
        group
            .MapPost(MapSabaId, "{sabaId}/mapping")
            .AddEndpointFilter<ValidatorFilter<MapSabaIdRequest>>();
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(MapSabaIdResponse), (int)HttpStatusCode.OK)]
    internal static async Task<IResult> MapSabaId(
        long sabaId,
        MapSabaIdRequest request,
        IMediator mediator,
        ILogAggregate logAggregate,
        IOptions<AppSetting> appSetting)
    {
        logAggregate.AppendHeader($"Log Aggregate: Licensee Map SabaId - SabaId: {sabaId} - CustId: {request.CustId}");

        var command = ObjectMapper.BuildMapSabaIdCommand(request, sabaId);
        var result = await mediator.Send(command);

        if (request.IsDebug || appSetting.Value.DebugMode)
        {
            logAggregate.SendLog(LogLevel.Information);
        }

        return result.Match(
            _ => Results.Ok(new MapSabaIdResponse
            {
                ErrorCode = 0,
                ErrorMessage = string.Empty
            }),
            (error, _) => Results.Ok(new MapSabaIdResponse
            {
                ErrorCode = error.Code,
                ErrorMessage = error.Message
            })
        );
    }
}