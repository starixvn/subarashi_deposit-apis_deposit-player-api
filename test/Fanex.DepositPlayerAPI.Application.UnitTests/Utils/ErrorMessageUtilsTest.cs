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
        var error = ServiceErrors.GetFromDbError(errorCode, errorMessage);
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
}