namespace Fanex.DepositPlayerAPI.Presentation.Endpoints;

using System.Net;
using Application.Common.Interfaces;
using Domain.Extensions;
using DTOs.Request;
using DTOs.Response;
using Extensions;
using Filters;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Utils;

public class ShadowAccountEndpoints : IEndpoints
{
    public string GroupName => "ShadowAccounts";

    public void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this);

        group
            .MapPost(CreateShadowAccount)
            .AddEndpointFilter<ValidatorFilter<CreateShadowAccountRequest>>();
    }

    [Produces("application/json")]
    [ProducesResponseType(typeof(CreateShadowAccountResponse), (int)HttpStatusCode.OK)]
    internal static async Task<IResult> CreateShadowAccount(
        CreateShadowAccountRequest request,
        IHttpContextAccessor contextAccessor,
        IMediator mediator,
        ILogAggregate logAggregate)
    {
        logAggregate.AppendHeader("Log Aggregate: Licensee Create Shadow Account");
        var command = ObjectMapper.MapShadowAccountCommand(request);
        var result = await mediator.Send(command);

        if (request.IsDebug)
        {
            logAggregate.SendLog(LogLevel.Information);
        }

        return result.Match(
            _ => Results.Ok(new CreateShadowAccountResponse
            {
                ErrorCode = 0,
                ErrorMessage = string.Empty,
                Result = result.Result
            }),
            (error, _) => Results.Ok(new CreateShadowAccountResponse
            {
                ErrorCode = error.Code,
                ErrorMessage = error.Message
            })
        );
    }
}