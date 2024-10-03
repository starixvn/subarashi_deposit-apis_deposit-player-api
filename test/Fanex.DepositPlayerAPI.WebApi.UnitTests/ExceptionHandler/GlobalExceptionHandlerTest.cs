namespace Fanex.DepositPlayerAPI.WebApi.UnitTests.ExceptionHandler;

using System.Net;
using FluentAssertions;
using Logging;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NSubstitute;
using WebApi.ExceptionHandler;

public class GlobalExceptionHandlerTest
{
    private readonly GlobalExceptionHandler _handler;
    private readonly ILogger _logger;

    public GlobalExceptionHandlerTest()
    {
        _logger = Substitute.For<ILogger>();
        _handler = new GlobalExceptionHandler(_logger);
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldHandleExceptions_ReturnCorrectData()
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
            code = 500,
            type = "",
            message = "An internal server error occurred while a request was processed."
        });

        // Act
        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        await _logger.Received().ErrorAsync(Arg.Is<string>(msg => msg.Contains("Test exception")), exception);
        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        httpContext.Response.ContentType.Should().Be("application/json; charset=utf-8");
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var bodyContent = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        bodyContent.Should().Be(expectedMsg);
    }
}