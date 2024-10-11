namespace Fanex.DepositPlayerAPI.Application.UnitTests.Utils;

using Application.Utils;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

public class ServerDbConfigUtilsTest
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IHttpContextAccessor _httpContextAccessor = Substitute.For<HttpContextAccessor>();

    [Fact]
    public void SetServerDbConfigToHttpContext_Should_SetCorrectValue()
    {
        // Arrange
        var data = _fixture.Create<ServerDbConfig>();
        var httpContext = Substitute.For<HttpContext>();
        _httpContextAccessor.HttpContext = httpContext;

        // Action
        ServerDbConfigUtils.SetServerDbConfigToHttpContext(data, _httpContextAccessor);

        // Assert
        httpContext.Items.Received(1)["CustDbGroupIdKey"] = data.CustDbGroupId;
        httpContext.Items.Received(1)["SiteDbGroupIdKey"] = data.SiteDbGroupId;
        httpContext.Items.Received(1)["CustDbServerKey"] = data.CustDbServer;
        httpContext.Items.Received(1)["SiteDbServerKey"] = data.SiteDbServer;
        httpContext.Items.Received(1)["SiteKey"] = data.Site;
    }

    [Fact]
    public void GetServerDbConfigFromHttpContext_Should_SetCorrectValue()
    {
        // Arrange
        var data = _fixture.Create<ServerDbConfig>();
        var httpContext = Substitute.For<HttpContext>();
        _httpContextAccessor.HttpContext = httpContext;
        SetupDataReturn(httpContext, data);

        // Action
        var result = ServerDbConfigUtils.GetServerDbConfigFromHttpContext(_httpContextAccessor);

        // Assert
        result.Should().BeEquivalentTo(data, options => options.Excluding(x => x.CustId));
    }

    private void SetupDataReturn(HttpContext mockHttpContext, ServerDbConfig serverDbConfig)
    {
        mockHttpContext.Items.TryGetValue("CustDbGroupIdKey", out _).Returns(c =>
        {
            c[1] = serverDbConfig.CustDbGroupId;
            return true;
        });

        mockHttpContext.Items.TryGetValue("SiteDbGroupIdKey", out _).Returns(c =>
        {
            c[1] = serverDbConfig.SiteDbGroupId;
            return true;
        });

        mockHttpContext.Items.TryGetValue("CustDbServerKey", out _).Returns(c =>
        {
            c[1] = serverDbConfig.CustDbServer;
            return true;
        });

        mockHttpContext.Items.TryGetValue("SiteDbServerKey", out _).Returns(c =>
        {
            c[1] = serverDbConfig.SiteDbServer;
            return true;
        });

        mockHttpContext.Items.TryGetValue("SiteKey", out _).Returns(c =>
        {
            c[1] = serverDbConfig.Site;
            return true;
        });
    }
}