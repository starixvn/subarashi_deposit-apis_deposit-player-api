namespace Fanex.DepositPlayerAPI.Application.UnitTests.Extensions;

using Application.Extensions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

public class HttpContextExtensionTest
{
    private readonly HttpContext _httpContext = Substitute.For<HttpContext>();

    [Fact]
    public void GetSite_ShouldReturnSite()
    {
        // Arrange
        var site = "TestSite";
        AssignMockValue("SiteKey", site);

        // Act
        var result = _httpContext.GetSite();

        // Assert
        Assert.Equal(site, result);
    }

    [Fact]
    public void GetSiteDbServer_ShouldReturnSiteDbServer()
    {
        // Arrange
        var siteDbServer = "TestSiteDbServer";
        AssignMockValue("SiteDbServerKey", siteDbServer);

        // Act
        var result = _httpContext.GetSiteDbServer();

        // Assert
        Assert.Equal(siteDbServer, result);
    }

    [Fact]
    public void GetCustDbServer_ShouldReturnCustDbServer()
    {
        // Arrange
        var custDbServer = "TestCustDbServer";
        AssignMockValue("CustDbServerKey", custDbServer);

        // Act
        var result = _httpContext.GetCustDbServer();

        // Assert
        Assert.Equal(custDbServer, result);
    }

    [Fact]
    public void GetSiteDbGroupId_ShouldReturnSiteDbGroupId()
    {
        // Arrange
        var siteDbGroupId = 123;
        AssignMockValue("SiteDbGroupIdKey", siteDbGroupId.ToString());

        // Act
        var result = _httpContext.GetSiteDbGroupId();

        // Assert
        Assert.Equal(siteDbGroupId, result);
    }

    [Fact]
    public void GetCustDbGroupId_ShouldReturnCustDbGroupId()
    {
        // Arrange
        var custDbGroupId = 456;
        AssignMockValue("CustDbGroupIdKey", custDbGroupId.ToString());

        // Act
        var result = _httpContext.GetCustDbGroupId();

        // Assert
        Assert.Equal(custDbGroupId, result);
    }

    [Fact]
    public void SetSite_ShouldSetSite()
    {
        // Arrange
        var site = "TestSite";

        // Act
        _httpContext.SetSite(site);

        // Assert
        Assert.Equal(site, _httpContext.Items["SiteKey"]);
    }

    [Fact]
    public void SetSiteDbServer_ShouldSetSiteDbServer()
    {
        // Arrange
        var siteDbServer = "TestSiteDbServer";

        // Act
        _httpContext.SetSiteDbServer(siteDbServer);

        // Assert
        Assert.Equal(siteDbServer, _httpContext.Items["SiteDbServerKey"]);
    }

    [Fact]
    public void SetCustDbServer_ShouldSetCustDbServer()
    {
        // Arrange
        var custDbServer = "TestCustDbServer";

        // Act
        _httpContext.SetCustDbServer(custDbServer);

        // Assert
        Assert.Equal(custDbServer, _httpContext.Items["CustDbServerKey"]);
    }

    [Fact]
    public void SetSiteDbGroupId_ShouldSetSiteDbGroupId()
    {
        // Arrange
        var siteDbGroupId = 123;

        // Act
        _httpContext.SetSiteDbGroupId(siteDbGroupId);

        // Assert
        Assert.Equal(siteDbGroupId, _httpContext.Items["SiteDbGroupIdKey"]);
    }

    [Fact]
    public void SetCustDbGroupId_ShouldSetCustDbGroupId()
    {
        // Arrange
        var custDbGroupId = 456;

        // Act
        _httpContext.SetCustDbGroupId(custDbGroupId);

        // Assert
        Assert.Equal(custDbGroupId, _httpContext.Items["CustDbGroupIdKey"]);
    }

    private void AssignMockValue(object key, object value)
    {
        _httpContext.Items.TryGetValue(key, out Arg.Any<object>()).Returns(x =>
        {
            x[1] = value;
            return true;
        });
    }
}