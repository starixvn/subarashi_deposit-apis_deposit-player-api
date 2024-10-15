namespace Fanex.DepositPlayerAPI.WebApi.Startup;

using System.Net.Mime;
using Domain.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

public static class WebApplicationSetup
{
    public static WebApplication ConfigureExceptionHandler(this WebApplication app, IConfiguration configuration)
    {
        var appSetting = configuration.GetSection(AppSetting.SettingName).Get<AppSetting>();

        if (appSetting.UseErrorHandler)
        {
            app.UseExceptionHandler(opts => { });
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        return app;
    }

    public static WebApplication ConfigureSwagger(this WebApplication app, IConfiguration configuration)
    {
        var appSetting = configuration.GetSection(AppSetting.SettingName).Get<AppSetting>();

        if (appSetting.UseSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.EnableTryItOutByDefault();
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Deposit Player API v{appSetting.Version}");
            });
        }

        return app;
    }

    public static IApplicationBuilder ConfigureHealthCheck(this IApplicationBuilder app)
    {
        Func<HttpContext, HealthReport, Task> customResponseWriter = async (context, report) =>
        {
            var result = JsonConvert.SerializeObject(new
            {
                status = report.Status.ToString(),
                monitors = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = Enum.GetName(typeof(HealthStatus), e.Value.Status),
                    description = e.Value.Description,
                    values = e.Value.Data
                })
            }, Formatting.Indented);

            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(result);
        };

        app.UseHealthChecks("/whoami", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            ResponseWriter = customResponseWriter
        });

        return app;
    }
}