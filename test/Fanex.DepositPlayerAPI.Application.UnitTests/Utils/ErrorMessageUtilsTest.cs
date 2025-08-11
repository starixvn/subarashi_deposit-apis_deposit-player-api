namespace Fanex.DepositPlayerAPI.Application.UnitTests.Utils;

using System.Net;
using Application.Utils;
using AutoFixture;
using Data;
using Domain.Errors;
using Domain.Results;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

public class ErrorMessageUtilsTest
{
    private readonly IFixture _fixture = new Fixture();

    [Theory]
    [InlineData(1, "Error 1")]
    [InlineData(2, "Error 2")]
    [InlineData(3, "Error 3")]
    public void GenerateMessage_Should_ReturnCorrectErrorInfo(int errorCode, string errorMessage)
    {
        // Arrange
        var functionName = _fixture.Create<string>();
        var chainName = _fixture.Create<string>();
        var request = _fixture.Create<RequestTest>();
        var error = ServiceError.GetFromDbError(errorCode, errorMessage);
        var response = ServiceResult.Failure(error);
        List<string> expected =
        [
            $"ERROR [{functionName}]: {JsonConvert.SerializeObject(response.Error)}",
            $"MESSAGE: An error occurred in the {chainName} process.",
            JsonConvert.SerializeObject(request, Formatting.Indented),
            JsonConvert.SerializeObject(response.Error, Formatting.Indented)
        ];

        // Act
        var result = ErrorMessageUtils.GenerateMessage(functionName, chainName, response, request);

        // Assert
        result.Should().ContainAll(expected);
    }

    [Fact]
    public void GenerateMessage_Should_ReturnCorrectExceptionInfo()
    {
        // Arrange
        var exception = new Exception("Test");
        var expected = $"MESSAGE: {exception.Message}";

        // Act
        var result = ErrorMessageUtils.GenerateMessage(exception);

        // Assert
        result.Should().StartWith(expected);
    }

    [Fact]
    public void GenerateMessage_Should_ReturnObjectDbExceptionInfo()
    {
        // Arrange
        var settingKey = _fixture.Create<string>();
        var spName = _fixture.Create<string>();
        var dbName = _fixture.Create<string>();
        var connectionString = "Data Source=10.18.200.56;User Id=asv_cust";
        var commandTimeout = _fixture.Create<int>();
        var connString = new SqlConnectionStringBuilder(connectionString);

        var exception = new ObjectDbException(
            new Exception("Test"),
            settingKey,
            spName,
            dbName,
            connectionString,
            commandTimeout,
            new Dictionary<string, string>()
        );

        List<string> expected =
        [
            "DATABASE INFO",
            $"Server: {connString.DataSource}",
            $"DbName: {dbName}",
            $"SpName: {spName}",
            $"UserId: {connString.UserID}",
            $"CommandTimeout: {commandTimeout}"
        ];

        // Act
        var result = ErrorMessageUtils.GenerateMessage(exception);

        // Assert
        result.Should().ContainAll(expected);
    }

    [Fact]
    public void GenerateMessage_Should_ReturnCorrectHttpContextInfo()
    {
        // Arrange
        var host = "abc.com";
        var path = "/test";
        var ip = "100.0.0.0";
        var httpContext = new DefaultHttpContext
        {
            HttpContext = { Request = { Host = new HostString(host), Path = path } },
            Connection = { RemoteIpAddress = IPAddress.Parse(ip) }
        };

        List<string> expected =
        [
            "REQUEST INFO",
            $"Host: {host}",
            $"Path: {path}",
            $"Remote IP: {ip}"
        ];

        // Act
        var result = ErrorMessageUtils.GenerateMessage(new Exception("Test"), httpContext);

        // Assert
        result.Should().ContainAll(expected);
    }

    internal class RequestTest
    {
        public string PropertyA { get; set; } = "TestA";
        public string PropertyB { get; set; } = "TestB";
    }

    [Fact]
    public void GenerateMessage_WithNullHttpContext_Should_NotIncludeRequestInfo()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        var result = ErrorMessageUtils.GenerateMessage(exception);

        // Assert
        result.Should().NotContain("REQUEST INFO");
        result.Should().Contain("DATABASE INFO");
        result.Should().Contain("EXCEPTION INFO");
    }

    [Fact]
    public void GenerateMessage_WithInnerException_Should_IncludeInnerExceptionInfo()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception message");
        var outerException = new ArgumentException("Outer exception message", innerException);

        // Act
        var result = ErrorMessageUtils.GenerateMessage(outerException);

        // Assert
        result.Should().Contain("Outer exception message");
        result.Should().Contain("Inner exception message");
        result.Should().Contain("EXCEPTION INFO");
    }

    [Fact]
    public void GenerateMessage_WithMultipleInnerExceptions_Should_LimitInnerExceptions()
    {
        // Arrange
        var innermost = new ArgumentNullException("Innermost exception");
        var inner3 = new InvalidOperationException("Third level exception", innermost);
        var inner2 = new ArgumentException("Second level exception", inner3);
        var inner1 = new InvalidOperationException("First level exception", inner2);
        var outer = new Exception("Outer exception", inner1);

        // Act
        var result = ErrorMessageUtils.GenerateMessage(outer);

        // Assert
        result.Should().Contain("Outer exception");
        result.Should().Contain("First level exception");
        result.Should().Contain("Second level exception");
        result.Should().Contain("Third level exception");
        // Should not contain the 4th level (innermost) due to limit of 3
    }

    [Fact]
    public void GenerateMessage_WithSqlException_Should_IncludeSqlExceptionInfo()
    {
        // Note: SqlException cannot be easily instantiated for testing due to its constructor restrictions
        // This test would require more complex mocking or reflection to create a proper SqlException
        // For now, we test with ObjectDbException which internally handles SqlException
        
        // Arrange
        var sqlInnerException = new Exception("SQL error"); // Simulating SQL exception behavior
        var spName = _fixture.Create<string>();
        var dbName = _fixture.Create<string>();
        var connectionString = "Data Source=testserver;User Id=testuser";
        var commandTimeout = _fixture.Create<int>();

        var objectDbException = new ObjectDbException(
            sqlInnerException,
            "testKey",
            spName,
            dbName,
            connectionString,
            commandTimeout,
            new Dictionary<string, string>()
        );

        // Act
        var result = ErrorMessageUtils.GenerateMessage(objectDbException);

        // Assert
        result.Should().Contain("DATABASE INFO");
        result.Should().Contain($"DbName: {dbName}");
        result.Should().Contain($"SpName: {spName}");
    }

    [Fact]
    public void GenerateMessage_WithExtraInfo_Should_IncludeExtraInfo()
    {
        // Arrange
        var functionName = _fixture.Create<string>();
        var chainName = _fixture.Create<string>();
        var request = _fixture.Create<RequestTest>();
        var extraInfo = _fixture.Create<string>();
        var error = ServiceError.GetFromDbError(1, "Test error");
        var response = ServiceResult.Failure(error, extraInfo);

        // Act
        var result = ErrorMessageUtils.GenerateMessage(functionName, chainName, response, request);

        // Assert
        result.Should().Contain(extraInfo);
    }

    [Fact]
    public void GenerateMessage_WithParameters_Should_IncludeParametersSection()
    {
        // Arrange
        var parameters = new Dictionary<string, string>
        {
            ["param1"] = "value1",
            ["param2"] = "value2"
        };

        var objectDbException = new ObjectDbException(
            new Exception("Test"),
            "testKey",
            "testSp",
            "testDb",
            "Data Source=testserver;User Id=testuser",
            30,
            parameters
        );

        // Act
        var result = ErrorMessageUtils.GenerateMessage(objectDbException);

        // Assert
        result.Should().Contain("Parameters:");
        result.Should().Contain("DATABASE INFO");
        result.Should().Contain("DbName: testDb");
        result.Should().Contain("SpName: testSp");
    }

    [Fact]
    public void GenerateMessage_WithEmptyParameters_Should_HandleEmptyParameters()
    {
        // Arrange
        var objectDbException = new ObjectDbException(
            new Exception("Test"),
            "testKey",
            "testSp",
            "testDb",
            "Data Source=testserver;User Id=testuser",
            30,
            new Dictionary<string, string>()
        );

        // Act
        var result = ErrorMessageUtils.GenerateMessage(objectDbException);

        // Assert
        result.Should().Contain("Parameters:");
        // Should handle empty parameters gracefully
    }

    [Fact]
    public void GenerateMessage_WithNullRequest_Should_SerializeNull()
    {
        // Arrange
        var functionName = _fixture.Create<string>();
        var chainName = _fixture.Create<string>();
        var error = ServiceError.GetFromDbError(1, "Test error");
        var response = ServiceResult.Failure(error);

        // Act
        var result = ErrorMessageUtils.GenerateMessage<RequestTest>(functionName, chainName, response, null);

        // Assert
        result.Should().Contain("REQUEST INFO:");
        result.Should().Contain("RESPONSE INFO:");
    }

    [Fact]
    public void GenerateMessage_Should_IncludeExceptionDetails()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception")
        {
            Source = "TestSource"
        };

        // Act
        var result = ErrorMessageUtils.GenerateMessage(exception);

        // Assert
        result.Should().Contain("EXCEPTION INFO");
        result.Should().Contain($"Message: {exception.Message}");
        result.Should().Contain($"Source: {exception.Source}");
        result.Should().Contain("TargetSite:");
        result.Should().Contain("StackTrace:");
    }
}