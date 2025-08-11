namespace Fanex.DepositPlayerAPI.Application.UnitTests.Extensions;

using Application.Extensions;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

public class HttpContextExtensionTest
{
    private readonly HttpContext _httpContext = Substitute.For<HttpContext>();
    private readonly IFixture _fixture = new Fixture();

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

    // Error handling and edge case tests
    [Fact]
    public void GetSite_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((HttpContext)null).GetSite();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void GetSiteDbServer_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((HttpContext)null).GetSiteDbServer();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void GetCustDbServer_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((HttpContext)null).GetCustDbServer();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void GetSiteDbGroupId_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((HttpContext)null).GetSiteDbGroupId();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void GetCustDbGroupId_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((HttpContext)null).GetCustDbGroupId();
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void GetSite_WithMissingKey_Should_ThrowKeyNotFoundException()
    {
        // Arrange
        _httpContext.Items.TryGetValue("SiteKey", out Arg.Any<object>()).Returns(false);

        // Act & Assert
        var act = () => _httpContext.GetSite();
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("SiteKey not found in HttpContext");
    }

    [Fact]
    public void GetSiteDbGroupId_WithInvalidValue_Should_ReturnDefault()
    {
        // Arrange
        AssignMockValue("SiteDbGroupIdKey", "invalid_number");

        // Act
        var result = _httpContext.GetSiteDbGroupId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCustDbGroupId_WithInvalidValue_Should_ReturnDefault()
    {
        // Arrange
        AssignMockValue("CustDbGroupIdKey", "invalid_number");

        // Act
        var result = _httpContext.GetCustDbGroupId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetSiteDbGroupId_WithNullValue_Should_ReturnDefault()
    {
        // Arrange
        AssignMockValue("SiteDbGroupIdKey", null);

        // Act
        var result = _httpContext.GetSiteDbGroupId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCustDbGroupId_WithNullValue_Should_ReturnDefault()
    {
        // Arrange
        AssignMockValue("CustDbGroupIdKey", null);

        // Act
        var result = _httpContext.GetCustDbGroupId();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void SetSite_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Arrange
        var site = _fixture.Create<string>();

        // Act & Assert
        var act = () => ((HttpContext)null).SetSite(site);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void SetSiteDbServer_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Arrange
        var server = _fixture.Create<string>();

        // Act & Assert
        var act = () => ((HttpContext)null).SetSiteDbServer(server);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void SetCustDbServer_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Arrange
        var server = _fixture.Create<string>();

        // Act & Assert
        var act = () => ((HttpContext)null).SetCustDbServer(server);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void SetSiteDbGroupId_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Arrange
        var groupId = _fixture.Create<int>();

        // Act & Assert
        var act = () => ((HttpContext)null).SetSiteDbGroupId(groupId);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void SetCustDbGroupId_WithNullHttpContext_Should_ThrowArgumentNullException()
    {
        // Arrange
        var groupId = _fixture.Create<int>();

        // Act & Assert
        var act = () => ((HttpContext)null).SetCustDbGroupId(groupId);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("context")
            .WithMessage("HttpContext should not be null!*");
    }

    [Fact]
    public void GetValueByKey_WithNullValueInItems_Should_ReturnNull()
    {
        // Arrange
        AssignMockValue("SiteKey", null);

        // Act
        var result = _httpContext.GetSite();

        // Assert
        result.Should().BeNull();
    }
}