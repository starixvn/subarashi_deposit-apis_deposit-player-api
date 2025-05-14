namespace Fanex.DepositPlayerAPI.Presentation.UnitTests.Endpoints;

using Application.Commands;
using Application.Services;
using AutoFixture;
using Domain.Errors;
using Domain.Options;
using Domain.Results;
using DTOs.Request;
using DTOs.Response;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Presentation.Endpoints;

public class SabaIdEndpointsTest
{
    private readonly IOptions<AppSetting> _appSetting = Substitute.For<IOptions<AppSetting>>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogAggregate _logAggregate = Substitute.For<ILogAggregate>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public SabaIdEndpointsTest()
    {
        _appSetting.Value.Returns(new AppSetting { DebugMode = false });
    }

    [Fact]
    public async Task MapSabaId_Should_ReturnSuccess()
    {
        // Arrange
        var sabaId = _fixture.Create<long>();
        var request = _fixture.Create<MapSabaIdRequest>();
        _mediator
            .Send(Arg.Any<MapSabaIdCommand>())
            .Returns(ServiceResult.Success());

        // Action
        var result = await SabaIdEndpoints.MapSabaId(sabaId, request, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<MapSabaIdResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(0);
        okResult.Value.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task MapSabaId_Should_ReturnFailure()
    {
        // Arrange
        var sabaId = _fixture.Create<long>();
        var request = _fixture.Create<MapSabaIdRequest>();
        _mediator
            .Send(Arg.Any<MapSabaIdCommand>())
            .Returns(ServiceResult.Failure(ServiceError.SystemError));

        // Action
        var result = await SabaIdEndpoints.MapSabaId(sabaId, request, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<MapSabaIdResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(ServiceError.SystemError.Code);
        okResult.Value.ErrorMessage.Should().Be(ServiceError.SystemError.Message);
    }

    [Fact]
    public async Task MapSabaId_Should_LogAggregate_WhenDebugIsTrue()
    {
        // Arrange
        var sabaId = _fixture.Create<long>();
        var request = _fixture.Build<MapSabaIdRequest>().With(x => x.IsDebug, true).Create();

        _mediator
            .Send(Arg.Any<MapSabaIdCommand>())
            .Returns(ServiceResult.Success());

        // Action
        await SabaIdEndpoints.MapSabaId(sabaId, request, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task MapSabaId_Should_LogAggregate_WhenDebugModeIsTrue()
    {
        // Arrange
        _appSetting.Value.Returns(new AppSetting { DebugMode = true });
        var sabaId = _fixture.Create<long>();
        var request = _fixture.Build<MapSabaIdRequest>().With(x => x.IsDebug, false).Create();

        _mediator
            .Send(Arg.Any<MapSabaIdCommand>())
            .Returns(ServiceResult.Success());

        // Action
        await SabaIdEndpoints.MapSabaId(sabaId, request, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task MapSabaId_Should_Not_LogAggregate_WhenDebugIsFalse()
    {
        // Arrange
        var sabaId = _fixture.Create<long>();
        var request = _fixture.Build<MapSabaIdRequest>().With(x => x.IsDebug, false).Create();

        _mediator
            .Send(Arg.Any<MapSabaIdCommand>())
            .Returns(ServiceResult.Success());

        // Action
        await SabaIdEndpoints.MapSabaId(sabaId, request, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.DidNotReceive().SendLog(LogLevel.Information);
    }
}