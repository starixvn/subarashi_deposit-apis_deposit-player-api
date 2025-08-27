namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Services;

using Application.Services;
using AutoFixture;
using Domain.Entities;
using Domain.Params;
using Domain.Repositories;
using Infrastructure.Services;
using Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class BackgroundTaskTest
{
    private readonly BackgroundTaskDependencies _dependencies;
    private readonly IBackgroundTask _backgroundTask;
    private readonly IFixture _fixture;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceScope _serviceScope;

    public BackgroundTaskTest()
    {
        _fixture = new Fixture();
        var rollbackRepository = Substitute.For<IRollbackRepository>();
        var logDbRepository = Substitute.For<ILogDbRepository>();
        var affiliateRepository = Substitute.For<IAffiliateRepository>();
        var commissionRepository = Substitute.For<ICommissionRepository>();
        var balanceRepository = Substitute.For<IBalanceRepository>();
        
        _dependencies = new BackgroundTaskDependencies(
            rollbackRepository,
            logDbRepository,
            affiliateRepository,
            commissionRepository,
            balanceRepository);
            
        _logger = Substitute.For<ILogger>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _serviceScope = Substitute.For<IServiceScope>();
        _backgroundTask = new BackgroundTask(
            _dependencies,
            _logger,
            _scopeFactory,
            _httpContextAccessor);

        _scopeFactory.CreateScope().Returns(_serviceScope);
        var httpContextAccessorStub = Substitute.For<IHttpContextAccessor>();
        _serviceScope.ServiceProvider.GetService<IHttpContextAccessor>().Returns(httpContextAccessorStub);

        _httpContextAccessor.HttpContext = new DefaultHttpContext();
        var serverDbConfig = _fixture.Create<ServerDbConfig>();
        SetupServerDbConfig(serverDbConfig, _httpContextAccessor);
    }

    private void SetupServerDbConfig(ServerDbConfig serverDbConfig, IHttpContextAccessor httpContextAccessorStub)
    {
        httpContextAccessorStub.HttpContext.Items["CustDbGroupIdKey"] = serverDbConfig.CustDbGroupId;
        httpContextAccessorStub.HttpContext.Items["CustDbServerKey"] = serverDbConfig.CustDbServer;
        httpContextAccessorStub.HttpContext.Items["SiteDbGroupIdKey"] = serverDbConfig.SiteDbGroupId;
        httpContextAccessorStub.HttpContext.Items["SiteDbServerKey"] = serverDbConfig.SiteDbServer;
        httpContextAccessorStub.HttpContext.Items["SiteKey"] = serverDbConfig.Site;
    }

    #region InsertNewPlayerAffiliateInfo

    [Fact]
    public async Task InsertNewPlayerAffiliateInfo_Should_CallInsertPlayerAffiliateAsync()
    {
        // Arrange
        var param = _fixture.Create<NewPlayerAffiliateParam>();
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.InsertNewPlayerAffiliateInfo(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.AffiliateRepository.Received(1).InsertPlayerAffiliateAsync(param);
    }

    [Fact]
    public async Task InsertNewPlayerAffiliateInfo_Should_LogError_WhenInsertPlayerAffiliateAsyncThrowException()
    {
        // Arrange
        var param = _fixture.Create<NewPlayerAffiliateParam>();
        var processName = _fixture.Create<string>();
        _dependencies.AffiliateRepository.InsertPlayerAffiliateAsync(Arg.Any<NewPlayerAffiliateParam>()).Throws<Exception>();

        // Action
        _backgroundTask.InsertNewPlayerAffiliateInfo(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.AffiliateRepository.Received(1).InsertPlayerAffiliateAsync(param);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion

    #region RollbackInfoNewPlayer

    [Fact]
    public async Task RollbackInfoNewPlayer_Should_Not_CallRollback_WhenCustIdIsInvalid()
    {
        // Arrange
        var custId = -1;
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerAsync(custId);
    }

    [Fact]
    public async Task RollbackInfoNewPlayer_Should_CallRollbackNewPlayerAsync()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
    }

    [Fact]
    public async Task RollbackInfoNewPlayer_Should_LogError_WhenRollbackNewPlayerAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _dependencies.RollbackRepository.RollbackNewPlayerAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion

    #region RollbackFullNewPlayer

    [Fact]
    public async Task RollbackFullNewPlayer_Should_Not_CallRollback_WhenCustIdIsInvalid()
    {
        // Arrange
        var custId = -1;
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerSettingAsync(Arg.Any<int>());
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerUserInfoAsync(Arg.Any<int>());
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_Call3RollBackFunctions()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerUserInfoAsync(custId);
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_LogError_WhenRollbackNewPlayerSettingAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _dependencies.RollbackRepository.RollbackNewPlayerSettingAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerUserInfoAsync(Arg.Any<int>());
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerAsync(Arg.Any<int>());
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_LogError_WhenRollbackNewPlayerUserInfoAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _dependencies.RollbackRepository.RollbackNewPlayerUserInfoAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerUserInfoAsync(custId);
        await _dependencies.RollbackRepository.DidNotReceive().RollbackNewPlayerAsync(Arg.Any<int>());
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_LogError_WhenRollbackNewPlayerAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _dependencies.RollbackRepository.RollbackNewPlayerAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerUserInfoAsync(custId);
        await _dependencies.RollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion

    #region WriteLogData

    [Fact]
    public async Task WriteLogData_Should_CallWriteLogDataDBLOGAsync()
    {
        // Arrange
        var logData = _fixture.Create<LogData>();
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.WriteLogData(processName, logData);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.LogDbRepository.Received(1).WriteLogDataDBLOGAsync(logData);
        await _dependencies.LogDbRepository.DidNotReceive().WriteLogDataDBCUSTAsync(logData);
    }

    [Fact]
    public async Task WriteLogData_Should_CallWriteLogDataDBCUSTAsync_WhenWriteLogDataDBLOGAsyncThrowException()
    {
        // Arrange
        var logData = _fixture.Create<LogData>();
        var processName = _fixture.Create<string>();
        _dependencies.LogDbRepository.WriteLogDataDBLOGAsync(Arg.Any<LogData>()).Throws<Exception>();

        // Action
        _backgroundTask.WriteLogData(processName, logData);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.LogDbRepository.Received(1).WriteLogDataDBLOGAsync(logData);
        await _dependencies.LogDbRepository.Received(1).WriteLogDataDBCUSTAsync(logData);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion

    #region WriteLogTracking

    [Fact]
    public async Task WriteLogTracking_Should_CallWriteLogTrackingDBLOGAsync()
    {
        // Arrange
        var logTracking = _fixture.Create<LogTracking>();
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.WriteLogTracking(processName, logTracking);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.LogDbRepository.Received(1).WriteLogTrackingDBLOGAsync(logTracking);
    }

    [Fact]
    public async Task WriteLogTracking_Should_LogError_WhenWriteLogTrackingDBLOGAsyncThrowException()
    {
        // Arrange
        var logTracking = _fixture.Create<LogTracking>();
        var processName = _fixture.Create<string>();
        _dependencies.LogDbRepository.WriteLogTrackingDBLOGAsync(Arg.Any<LogTracking>()).Throws<Exception>();

        // Action
        _backgroundTask.WriteLogTracking(processName, logTracking);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.LogDbRepository.Received(1).WriteLogTrackingDBLOGAsync(logTracking);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion

    #region InsertDefaultCommLicenseeDb

    [Fact]
    public async Task InsertDefaultCommLicenseeDb_Should_CallInsertDefaultCommAsync()
    {
        // Arrange
        var param = _fixture.Create<InsertDefaultCommissionParam>();
        var processName = _fixture.Create<string>();

        // Action
        _backgroundTask.InsertDefaultCommLicenseeDb(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.CommissionRepository.Received(1).InsertDefaultCommAsync(param);
    }

    [Fact]
    public async Task InsertDefaultCommLicenseeDb_Should_LogError_WhenInsertDefaultCommAsyncThrowException()
    {
        // Arrange
        var param = _fixture.Create<InsertDefaultCommissionParam>();
        var processName = _fixture.Create<string>();
        _dependencies.CommissionRepository.InsertDefaultCommAsync(Arg.Any<InsertDefaultCommissionParam>()).Throws<Exception>();

        // Action
        _backgroundTask.InsertDefaultCommLicenseeDb(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _dependencies.CommissionRepository.Received(1).InsertDefaultCommAsync(param);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion
}