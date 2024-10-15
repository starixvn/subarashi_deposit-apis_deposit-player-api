using Fanex.DepositPlayerAPI.Presentation;
using Fanex.DepositPlayerAPI.WebApi.Startup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddDependencyInjection(builder.Configuration);

var app = builder.Build();

app.ConfigureHealthCheck();
app.UseForwardedHeaders();

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    builder => builder.UseHttpLogging());

app.ConfigureExceptionHandler(builder.Configuration);

app.ConfigureSwagger(builder.Configuration);

app.UseRouting();

app.UseCors();

app.UseRequestLocalization();

app.MapGet("/", () => "Deposit Player API is running ...").ExcludeFromDescription();

app.MapEndpoints();

app.Run();