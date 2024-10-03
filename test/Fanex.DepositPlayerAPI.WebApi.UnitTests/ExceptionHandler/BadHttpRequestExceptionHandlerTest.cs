namespace Fanex.DepositPlayerAPI.WebApi.UnitTests.ExceptionHandler;

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WebApi.ExceptionHandler;

public class BadHttpRequestExceptionHandlerTest
{
    private readonly BadHttpRequestExceptionHandler _handler = new();

    [Fact]
    public async Task BadHttpRequestException_ShouldHandleExceptions_WhenBadHttpRequestIsThrown()
    {
        // Arrange
        var exception = new BadHttpRequestException("Test exception");
        var httpContext = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var expectedMsg = JsonConvert.SerializeObject(new
        {
            code = 400,
            type = "INVALID_PARAMETER_TYPE",
            message = "The input parameters are of an incorrect type."
        });

        // Act
        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var bodyContent = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        bodyContent.Should().Be(expectedMsg);
    }


    [Fact]
    public async Task BadHttpRequestException_ShouldNotHandleExceptions_WhenOtherExceptionIsThrown()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var httpContext = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var expectedMsg = JsonConvert.SerializeObject(new
        {
            code = 400,
            type = "INVALID_PARAMETER_TYPE",
            message = "The input parameters are of an incorrect type."
        });

        // Act
        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        httpContext.Response.StatusCode.Should().NotBe((int)HttpStatusCode.BadRequest);
        httpContext.Response.ContentType.Should().NotBe("application/json; charset=utf-8");
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var bodyContent = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        bodyContent.Should().NotBe(expectedMsg);
    }
}