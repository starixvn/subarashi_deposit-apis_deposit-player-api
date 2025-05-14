namespace Fanex.DepositPlayerAPI.Presentation.UnitTests.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using Presentation.Endpoints;
using Presentation.Extensions;

public class IEndpointRouteBuilderExtensionsTest
{
    [Fact]
    public void MapGroup_Should_MapGroupCorrectly()
    {
        // Arrange
        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        var endpoints = Substitute.For<IEndpoints>();
        var groupName = "TestGroup";
        endpoints.GroupName.ToLowerInvariant().Returns(groupName);

        // Act
        endpointRouteBuilder.MapGroup(endpoints);

        // Assert
        endpointRouteBuilder.Received(1).MapGroup(groupName);
    }

    [Fact]
    public void MapGet_Should_MapGetCorrectly()
    {
        // Arrange
        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        RequestDelegate handler = context => context.Response.WriteAsync("Hello, world!");
        var pattern = "testPattern";

        // Act
        endpointRouteBuilder.MapGet(handler, pattern);

        // Assert
        endpointRouteBuilder.Received().MapGet(pattern, handler);
    }

    [Fact]
    public void MapPost_Should_MapPostCorrectly()
    {
        // Arrange
        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        RequestDelegate handler = context => context.Response.WriteAsync("Hello, world!");
        var pattern = "testPattern";

        // Act
        endpointRouteBuilder.MapPost(handler, pattern);

        // Assert
        endpointRouteBuilder.Received().MapPost(pattern, handler);
    }

    [Fact]
    public void MapPut_Should_MapPutCorrectly()
    {
        // Arrange
        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        RequestDelegate handler = context => context.Response.WriteAsync("Hello, world!");
        var pattern = "testPattern";

        // Act
        endpointRouteBuilder.MapPut(handler, pattern);

        // Assert
        endpointRouteBuilder.Received().MapPut(pattern, handler);
    }

    [Fact]
    public void MapDelete_Should_MapDeleteCorrectly()
    {
        // Arrange
        var endpointRouteBuilder = Substitute.For<IEndpointRouteBuilder>();
        RequestDelegate handler = context => context.Response.WriteAsync("Hello, world!");
        var pattern = "testPattern";

        // Act
        endpointRouteBuilder.MapDelete(handler, pattern);

        // Assert
        endpointRouteBuilder.Received().MapDelete(pattern, handler);
    }
}