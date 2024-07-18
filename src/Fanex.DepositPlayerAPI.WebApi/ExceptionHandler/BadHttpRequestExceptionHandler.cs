namespace Fanex.DepositPlayerAPI.WebApi.ExceptionHandler;

using System.Net;
using Microsoft.AspNetCore.Diagnostics;

public sealed class BadHttpRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException)
        {
            return false;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        var result = new
        {
            code = 400,
            type = "INVALID_PARAMETER_TYPE",
            message = "The input parameters are of an incorrect type."
        };

        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);
        return true;
    }
}