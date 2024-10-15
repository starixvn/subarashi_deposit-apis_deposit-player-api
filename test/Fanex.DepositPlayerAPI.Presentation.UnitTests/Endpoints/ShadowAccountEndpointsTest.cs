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

public class ShadowAccountEndpointsTest
{
    private readonly IOptions<AppSetting> _appSetting = Substitute.For<IOptions<AppSetting>>();
    private readonly IHttpContextAccessor _contextAccessor = Substitute.For<IHttpContextAccessor>();
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogAggregate _logAggregate = Substitute.For<ILogAggregate>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public ShadowAccountEndpointsTest()
    {
        _appSetting.Value.Returns(new AppSetting { DebugMode = false });
    }

    [Fact]
    public async Task CreateShadowAccount_Should_ReturnSuccess()
    {
        // Arrange
        var request = _fixture.Create<CreateShadowAccountRequest>();
        var data = _fixture.Create<ShadowAccount>();
        _mediator
            .Send(Arg.Any<NewShadowAccountCommand>())
            .Returns(ServiceResult<ShadowAccount>.Success(data));

        // Action
        var result = await ShadowAccountEndpoints.CreateShadowAccount(request, _contextAccessor, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<CreateShadowAccountResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(0);
        okResult.Value.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateShadowAccount_Should_ReturnFailure()
    {
        // Arrange
        var request = _fixture.Create<CreateShadowAccountRequest>();
        _mediator
            .Send(Arg.Any<NewShadowAccountCommand>())
            .Returns(ServiceResult<ShadowAccount>.Failure(ServiceErrors.SystemError));

        // Action
        var result = await ShadowAccountEndpoints.CreateShadowAccount(request, _contextAccessor, _mediator, _logAggregate, _appSetting);
        var okResult = (Ok<CreateShadowAccountResponse>)result;

        // Assert
        okResult.StatusCode.Should().Be(200);
        okResult.Value.ErrorCode.Should().Be(ServiceErrors.SystemError.Code);
        okResult.Value.ErrorMessage.Should().Be(ServiceErrors.SystemError.Message);
    }

    [Fact]
    public async Task ReactivatePlayer_Should_LogAggregate_WhenDebugIsTrue()
    {
        // Arrange
        var request = _fixture.Build<CreateShadowAccountRequest>().With(x => x.IsDebug, true).Create();
        var data = _fixture.Create<ShadowAccount>();
        _mediator
            .Send(Arg.Any<NewShadowAccountCommand>())
            .Returns(ServiceResult<ShadowAccount>.Success(data));

        // Action
        await ShadowAccountEndpoints.CreateShadowAccount(request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task ReactivatePlayer_Should_LogAggregate_WhenDebugModeIsTrue()
    {
        // Arrange
        _appSetting.Value.Returns(new AppSetting { DebugMode = true });
        var request = _fixture.Build<CreateShadowAccountRequest>().With(x => x.IsDebug, false).Create();
        var data = _fixture.Create<ShadowAccount>();
        _mediator
            .Send(Arg.Any<NewShadowAccountCommand>())
            .Returns(ServiceResult<ShadowAccount>.Success(data));

        // Action
        await ShadowAccountEndpoints.CreateShadowAccount(request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.Received(1).SendLog(LogLevel.Information);
    }

    [Fact]
    public async Task ReactivatePlayer_Should_Not_LogAggregate_WhenDebugIsFalse()
    {
        // Arrange
        var request = _fixture.Build<CreateShadowAccountRequest>().With(x => x.IsDebug, false).Create();
        var data = _fixture.Create<ShadowAccount>();
        _mediator
            .Send(Arg.Any<NewShadowAccountCommand>())
            .Returns(ServiceResult<ShadowAccount>.Success(data));

        // Action
        await ShadowAccountEndpoints.CreateShadowAccount(request, _contextAccessor, _mediator, _logAggregate, _appSetting);

        // Assert
        _logAggregate.DidNotReceive().SendLog(LogLevel.Information);
    }
}