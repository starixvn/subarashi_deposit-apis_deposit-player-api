namespace Fanex.DepositPlayerAPI.Application.UnitTests.Utils;

using System.Net;
using Application.Utils;
using AutoFixture;
using Data;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

public class ErrorMessageUtilsTest
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void GenerateMessage_Should_ReturnCorrectExceptionInfo()
    {
        // Arrange
        var exception = new Exception("Test");
        var expected = $"ErrMsg: {exception.Message}";

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
}