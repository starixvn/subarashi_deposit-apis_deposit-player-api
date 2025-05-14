namespace Fanex.DepositPlayerAPI.WebApi.Startup;

using System.Reflection;
using Application;
using Configurations;
using Domain.Options;
using ExceptionHandler;
using HealthChecks;
using HostedServices;
using Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSetting>(configuration.GetSection(AppSetting.SettingName));
        services.Configure<SentrySetting>(configuration.GetSection(SentrySetting.SettingName));

        services
            .AddApplication()
            .AddInfrastructure()
            .AddPresentation();

        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));

        services.Configure<RouteHandlerOptions>(options => { options.ThrowOnBadRequest = true; });

        services.AddHostedService<SetupDbServerConfigCacheHostedService>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.ForwardLimit = null;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services
            .AddHealthChecks()
            .AddCheck<ServerHealthCheck>("Server")
            .AddCheck<MemoryHealthCheck>("Memory")
            .AddCheck<VaultHealthCheck>("Vault");

        services.AddHttpContextAccessor();
        services.AddExceptionHandler<BadHttpRequestExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.InstallServices(configuration, typeof(IServiceInstaller).Assembly);

        return services;
    }

    private static void InstallServices(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        var serviceInstallers = assemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => typeof(IServiceInstaller).IsAssignableFrom(t) && t.IsClass)
            .Select(Activator.CreateInstance)
            .Cast<IServiceInstaller>();

        foreach (var serviceInstaller in serviceInstallers)
        {
            serviceInstaller.Install(services, configuration);
        }
    }
}