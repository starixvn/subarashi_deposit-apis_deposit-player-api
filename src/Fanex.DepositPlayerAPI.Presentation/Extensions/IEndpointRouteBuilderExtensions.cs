namespace Fanex.DepositPlayerAPI.Presentation.Extensions;

using System.Diagnostics.CodeAnalysis;
using Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class IEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapGroup(this IEndpointRouteBuilder app, IEndpoints group)
    {
        var groupName = group.GroupName;
        var groupPath = group.GroupName.ToLowerInvariant();

        var routeHandler = app
            .MapGroup($"/api/{groupPath}")
            //.WithGroupName(groupName) // use when have multiple version swagger
            .WithTags(groupName)
            .WithOpenApi();

        return routeHandler;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointType = typeof(IEndpoints);
        var assembly = endpointType.Assembly;

        var endpointTypes = assembly.GetExportedTypes()
            .Where(t => t.IsClass && endpointType.IsAssignableFrom(t));

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoints instance)
            {
                instance.Map(app);
            }
        }

        return app;
    }

    public static RouteHandlerBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        return builder.MapGet(pattern, handler)
            .WithName(handler.Method.Name);
    }

    public static RouteHandlerBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        return builder.MapPost(pattern, handler)
            .WithName(handler.Method.Name);
    }

    public static RouteHandlerBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        return builder.MapPut(pattern, handler)
            .WithName(handler.Method.Name);
    }

    public static RouteHandlerBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        return builder.MapDelete(pattern, handler)
            .WithName(handler.Method.Name);
    }
}