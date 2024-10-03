namespace Fanex.DepositPlayerAPI.WebApi.UnitTests.HealthChecks;

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using AutoFixture;
using FluentAssertions;
using Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NSubstitute;
using WebApi.HealthChecks;

public class ServerHealthCheckTest
{
    private readonly AppSetting _appSetting;
    private readonly Fixture _fixture;
    private readonly ServerHealthCheck _healthChecks;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerHealthCheckTest()
    {
        _fixture = new Fixture();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _appSetting = _fixture.Create<AppSetting>();
        _healthChecks = new ServerHealthCheck(_httpContextAccessor, Options.Create(_appSetting));
    }

    [Fact]
    public async Task ServerHealthCheck_Should_ReturnCorrectData()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(httpContext);
        var hostName = Dns.GetHostName();
        var serverIps = await Dns.GetHostAddressesAsync(hostName);
        var serverTime = DateTimeOffset.Now.LocalDateTime.ToString(CultureInfo.InvariantCulture);
        var lastUpdated = File.GetCreationTime(typeof(ServerHealthCheck).Assembly.Location).ToString(CultureInfo.InvariantCulture);

        var expected = new Dictionary<string, object>
        {
            { "Env", _appSetting.Environment },
            { "Version", _appSetting.Version },
            { "Host Name", hostName },
            { "Local Ip", httpContext.Connection.LocalIpAddress?.ToString()! },
            { "Local Port", httpContext.Connection.LocalPort! },
            { "Client Ip", httpContext.Connection.RemoteIpAddress?.ToString()! },
            { "Server Time", serverTime },
            { "Last Updated", lastUpdated },
            { "Host", httpContext.Request.Host },
            { "Schema", httpContext.Request.Scheme }
        };

        for (var i = 0; i < serverIps.Length; i++)
        {
            if (serverIps[i].AddressFamily == AddressFamily.InterNetwork)
            {
                expected.Add($"IP{i}", serverIps[i].ToString());
            }
        }

        // Action
        var result = await _healthChecks.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Server Info");
        result.Exception.Should().BeNull();
        result.Data.Should().BeEquivalentTo(expected);
    }
}