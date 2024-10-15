namespace Fanex.DepositPlayerAPI.Presentation.UnitTests.Endpoints;

using Application.Commands;
using Application.Common.Interfaces;
using AutoFixture;
using Domain.Entities;
using Domain.Errors;
using Domain.Options;
using Domain.Results;
using DTOs.Request;
using DTOs.Response;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Presentation.Endpoints;

public class PlayerEndpointsTest
{
    private readonly IOptions<AppSetting> _appSetting = Substitute.For<IOptions<AppSetting>>();
    private readonly IHttpContextAccessor _contextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogAggregate _logAggregate = Substitute.For<ILogAggregate>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public PlayerEndpointsTest()
    {
        _appSetting.Value.Returns(new AppSetting { DebugMode = false });
    }

    #region ReactivatePlayer

    [Fact]
    public async Task ReactivatePlayer_Should_ReturnSuccess()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var request = _fixture.Create<ReactivatePlayerRequest>();
        _mediator
            .Send(Arg.Any<ReactivatePlayerCommand>())
            .Returns(ServiceResult.Success());

        // Action
        var result = await PlayerEndpoints.ReactivatePlayer(custId, request, _contextAccessor, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<ReactivatePlayerResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(0);
        okResult.Value.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task ReactivatePlayer_Should_ReturnFailure()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var request = _fixture.Create<ReactivatePlayerRequest>();
        _mediator
            .Send(Arg.Any<ReactivatePlayerCommand>())
            .Returns(ServiceResult.Failure(ServiceErrors.SystemError));

        // Action
        var result = await PlayerEndpoints.ReactivatePlayer(custId, request, _contextAccessor, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<ReactivatePlayerResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(ServiceErrors.SystemError.Code);
        okResult.Value.ErrorMessage.Should().Be(ServiceErrors.SystemError.Message);
    }

    [Fact]
    public async Task ReactivatePlayer_Should_LogAggregate_WhenDebugIsTrue()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var request = _fixture.Build<ReactivatePlayerRequest>().With(x => x.IsDebug, true).Create();

        _mediator
            .Send(Arg.Any<ReactivatePlayerCommand>())
            .Returns(ServiceResult.Success());

        // Action
        await PlayerEndpoints.ReactivatePlayer(custId, request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task ReactivatePlayer_Should_LogAggregate_WhenDebugModeIsTrue()
    {
        // Arrange
        _appSetting.Value.Returns(new AppSetting { DebugMode = true });
        var custId = _fixture.Create<int>();
        var request = _fixture.Build<ReactivatePlayerRequest>().With(x => x.IsDebug, false).Create();

        _mediator
            .Send(Arg.Any<ReactivatePlayerCommand>())
            .Returns(ServiceResult.Success());

        // Action
        await PlayerEndpoints.ReactivatePlayer(custId, request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task ReactivatePlayer_Should_Not_LogAggregate_WhenDebugIsFalse()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var request = _fixture.Build<ReactivatePlayerRequest>().With(x => x.IsDebug, false).Create();

        _mediator
            .Send(Arg.Any<ReactivatePlayerCommand>())
            .Returns(ServiceResult.Success());

        // Action
        await PlayerEndpoints.ReactivatePlayer(custId, request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.DidNotReceive().SendLog(LogLevel.Information);
    }

    #endregion


    #region CreatePlayer

    [Fact]
    public async Task CreatePlayer_Should_ReturnSuccess()
    {
        // Arrange
        var request = _fixture.Create<CreatePlayerRequest>();
        var data = _fixture.Create<Player>();
        _mediator
            .Send(Arg.Any<NewPlayerCommand>())
            .Returns(ServiceResult<Player>.Success(data));

        // Action
        var result = await PlayerEndpoints.CreatePlayer(request, _contextAccessor, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<CreatePlayerResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(0);
        okResult.Value.ErrorMessage.Should().BeEmpty();
        okResult.Value.Result.Should().Be(data);
    }

    [Fact]
    public async Task CreatePlayer_Should_ReturnFailure()
    {
        // Arrange
        var request = _fixture.Create<CreatePlayerRequest>();
        _mediator
            .Send(Arg.Any<NewPlayerCommand>())
            .Returns(ServiceResult<Player>.Failure(ServiceErrors.SystemError));

        // Action
        var result = await PlayerEndpoints.CreatePlayer(request, _contextAccessor, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<CreatePlayerResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(ServiceErrors.SystemError.Code);
        okResult.Value.ErrorMessage.Should().Be(ServiceErrors.SystemError.Message);
        okResult.Value.Result.Should().BeNull();
    }

    [Fact]
    public async Task CreatePlayer_Should_LogAggregate_WhenDebugIsTrue()
    {
        // Arrange
        var request = _fixture.Build<CreatePlayerRequest>().With(x => x.IsDebug, true).Create();
        var data = _fixture.Create<Player>();
        _mediator
            .Send(Arg.Any<NewPlayerCommand>())
            .Returns(ServiceResult<Player>.Success(data));

        // Action
        await PlayerEndpoints.CreatePlayer(request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task CreatePlayer_Should_LogAggregate_WhenDebugModeIsTrue()
    {
        // Arrange
        _appSetting.Value.Returns(new AppSetting { DebugMode = true });
        var request = _fixture.Build<CreatePlayerRequest>().With(x => x.IsDebug, false).Create();
        var data = _fixture.Create<Player>();
        _mediator
            .Send(Arg.Any<NewPlayerCommand>())
            .Returns(ServiceResult<Player>.Success(data));

        // Action
        await PlayerEndpoints.CreatePlayer(request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task CreatePlayer_Should_Not_LogAggregate_WhenDebugIsFalse()
    {
        // Arrange
        var request = _fixture.Build<CreatePlayerRequest>().With(x => x.IsDebug, false).Create();
        var data = _fixture.Create<Player>();
        _mediator
            .Send(Arg.Any<NewPlayerCommand>())
            .Returns(ServiceResult<Player>.Success(data));

        // Action
        await PlayerEndpoints.CreatePlayer(request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.DidNotReceive().SendLog(LogLevel.Information);
    }

    #endregion
}