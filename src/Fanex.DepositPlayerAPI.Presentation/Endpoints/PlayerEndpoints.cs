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

public class PlayerEndpoints : IEndpoints
{
    public string GroupName => "Players";

    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this);

        group
            .MapPost(CreatePlayer)
            .AddEndpointFilter<ValidatorFilter<CreatePlayerRequest>>();

        group
            .MapPut(ReactivatePlayer, "{custId}")
            .AddEndpointFilter<ValidatorFilter<ReactivatePlayerRequest>>();
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(CreatePlayerResponse), (int)HttpStatusCode.OK)]
    internal static async Task<IResult> CreatePlayer(
        CreatePlayerRequest request,
        IHttpContextAccessor contextAccessor,
        IMediator mediator,
        ILogAggregate logAggregate,
        IOptions<AppSetting> appSetting)
    {
        logAggregate.AppendHeader("Log Aggregate: Licensee Create Player");

        var command = ObjectMapper.MapNewPlayerCommand(request);
        var result = await mediator.Send(command);

        if (request.IsDebug || appSetting.Value.DebugMode)
        {
            logAggregate.SendLog(LogLevel.Information);
        }

        return result.Match(
            _ => Results.Ok(new CreatePlayerResponse
            {
                ErrorCode = 0,
                ErrorMessage = string.Empty,
                Result = result.Result
            }),
            (error, _) => Results.Ok(new CreatePlayerResponse
            {
                ErrorCode = error.Code,
                ErrorMessage = error.Message
            })
        );
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(ReactivatePlayerResponse), (int)HttpStatusCode.OK)]
    internal static async Task<IResult> ReactivatePlayer(
        int custId,
        ReactivatePlayerRequest request,
        IHttpContextAccessor contextAccessor,
        IMediator mediator,
        ILogAggregate logAggregate,
        IOptions<AppSetting> appSetting)
    {
        logAggregate.AppendHeader("Log Aggregate: Licensee Reactivate Player");

        var command = ObjectMapper.MapReactivatePlayerCommand(request, custId);
        var result = await mediator.Send(command);

        if (request.IsDebug || appSetting.Value.DebugMode)
        {
            logAggregate.SendLog(LogLevel.Information);
        }

        return result.Match(
            _ => Results.Ok(new ReactivatePlayerResponse
            {
                ErrorCode = 0,
                ErrorMessage = string.Empty
            }),
            (error, _) => Results.Ok(new ReactivatePlayerResponse
            {
                ErrorCode = error.Code,
                ErrorMessage = error.Message
            })
        );
    }
}