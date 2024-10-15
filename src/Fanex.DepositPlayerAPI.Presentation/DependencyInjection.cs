namespace Fanex.DepositPlayerAPI.Presentation;

using Endpoints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        return services;
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
}