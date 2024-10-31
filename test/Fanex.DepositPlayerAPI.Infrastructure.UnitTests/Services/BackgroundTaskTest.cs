namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Services;

using Application.Services;
using AutoFixture;
using Domain.Entities;
using Domain.Params;
using Domain.Repositories;
using Domain.Results;
using Infrastructure.Services;
using Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

public class BackgroundTaskTest
{
    private readonly IAffiliateRepository _affiliateRepository;
    private readonly IBackgroundTask _backgroundTask;
    private readonly IBalanceRepository _balanceRepository;
    private readonly ICommissionRepository _commissionRepository;
    private readonly IFixture _fixture;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogDbRepository _logDbRepository;
    private readonly ILogger _logger;
    private readonly IRollbackRepository _rollbackRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceScope _serviceScope;

    public BackgroundTaskTest()
    {
        _fixture = new Fixture();
        _rollbackRepository = Substitute.For<IRollbackRepository>();
        _logDbRepository = Substitute.For<ILogDbRepository>();
        _affiliateRepository = Substitute.For<IAffiliateRepository>();
        _commissionRepository = Substitute.For<ICommissionRepository>();
        _balanceRepository = Substitute.For<IBalanceRepository>();
        _logger = Substitute.For<ILogger>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _serviceScope = Substitute.For<IServiceScope>();
        _backgroundTask = new BackgroundTask(
            _rollbackRepository,
            _logDbRepository,
            _affiliateRepository,
            _commissionRepository,
            _balanceRepository,
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

    #region ReinitBalanceReactivatePlayer

    [Fact]
    public async Task ReinitBalanceReactivatePlayer_Should_CallInitializeReactivatePlayerBalanceAndWriteLogTracking()
    {
        // Arrange
        var param = _fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var processName = _fixture.Create<string>();
        var logTracking = _fixture.Create<LogTracking>();
        _balanceRepository.InitializeReactivatePlayerBalanceAsync(param).Returns(ServiceResult<LogTracking>.Success(logTracking));

        // Action
        _backgroundTask.ReinitBalanceReactivatePlayer(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(param);
        await _logDbRepository.Received(1).WriteLogTrackingDBLOGAsync(logTracking);
    }

    [Fact]
    public async Task ReinitBalanceReactivatePlayer_Should_CallInitializeReactivatePlayerBalanceAndNotWriteLogTracking()
    {
        // Arrange
        var param = _fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var processName = _fixture.Create<string>();
        _balanceRepository.InitializeReactivatePlayerBalanceAsync(param).Returns(ServiceResult<LogTracking>.Success(null!));

        // Action
        _backgroundTask.ReinitBalanceReactivatePlayer(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(param);
        await _logDbRepository.DidNotReceive().WriteLogTrackingDBLOGAsync(Arg.Any<LogTracking>());
    }

    [Fact]
    public async Task ReinitBalanceReactivatePlayer_Should_LogError_WhenInitializeReactivatePlayerBalanceThrowException()
    {
        // Arrange
        var param = _fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var processName = _fixture.Create<string>();
        _balanceRepository.InitializeReactivatePlayerBalanceAsync(param).Throws<Exception>();

        // Action
        _backgroundTask.ReinitBalanceReactivatePlayer(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(param);
        await _logDbRepository.DidNotReceive().WriteLogTrackingDBLOGAsync(Arg.Any<LogTracking>());
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    [Fact]
    public async Task ReinitBalanceReactivatePlayer_Should_LogError_WhenWriteLogTrackingDBLOGAsyncThrowException()
    {
        // Arrange
        var param = _fixture.Create<InitializeReactivatePlayerBalanceParam>();
        var logTracking = _fixture.Create<LogTracking>();
        var processName = _fixture.Create<string>();
        _balanceRepository.InitializeReactivatePlayerBalanceAsync(param).Returns(ServiceResult<LogTracking>.Success(logTracking));
        _logDbRepository.WriteLogTrackingDBLOGAsync(Arg.Any<LogTracking>()).Throws<Exception>();

        // Action
        _backgroundTask.ReinitBalanceReactivatePlayer(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _balanceRepository.Received(1).InitializeReactivatePlayerBalanceAsync(param);
        await _logDbRepository.Received(1).WriteLogTrackingDBLOGAsync(logTracking);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion

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
        await _affiliateRepository.Received(1).InsertPlayerAffiliateAsync(param);
    }

    [Fact]
    public async Task InsertNewPlayerAffiliateInfo_Should_LogError_WhenInsertPlayerAffiliateAsyncThrowException()
    {
        // Arrange
        var param = _fixture.Create<NewPlayerAffiliateParam>();
        var processName = _fixture.Create<string>();
        _affiliateRepository.InsertPlayerAffiliateAsync(Arg.Any<NewPlayerAffiliateParam>()).Throws<Exception>();

        // Action
        _backgroundTask.InsertNewPlayerAffiliateInfo(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _affiliateRepository.Received(1).InsertPlayerAffiliateAsync(param);
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
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerAsync(custId);
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
        await _rollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
    }

    [Fact]
    public async Task RollbackInfoNewPlayer_Should_LogError_WhenRollbackNewPlayerAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _rollbackRepository.RollbackNewPlayerAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _rollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
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
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerSettingAsync(Arg.Any<int>());
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerUserInfoAsync(Arg.Any<int>());
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerAsync(Arg.Any<int>());
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
        await _rollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _rollbackRepository.Received(1).RollbackNewPlayerUserInfoAsync(custId);
        await _rollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_LogError_WhenRollbackNewPlayerSettingAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _rollbackRepository.RollbackNewPlayerSettingAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _rollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerUserInfoAsync(Arg.Any<int>());
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerAsync(Arg.Any<int>());
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_LogError_WhenRollbackNewPlayerUserInfoAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _rollbackRepository.RollbackNewPlayerUserInfoAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _rollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _rollbackRepository.Received(1).RollbackNewPlayerUserInfoAsync(custId);
        await _rollbackRepository.DidNotReceive().RollbackNewPlayerAsync(Arg.Any<int>());
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    [Fact]
    public async Task RollbackFullNewPlayer_Should_LogError_WhenRollbackNewPlayerAsyncThrowException()
    {
        // Arrange
        var custId = _fixture.Create<int>();
        var processName = _fixture.Create<string>();
        _rollbackRepository.RollbackNewPlayerAsync(custId).Throws<Exception>();

        // Action
        _backgroundTask.RollbackFullNewPlayer(processName, custId);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _rollbackRepository.Received(1).RollbackNewPlayerSettingAsync(custId);
        await _rollbackRepository.Received(1).RollbackNewPlayerUserInfoAsync(custId);
        await _rollbackRepository.Received(1).RollbackNewPlayerAsync(custId);
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
        await _logDbRepository.Received(1).WriteLogDataDBLOGAsync(logData);
        await _logDbRepository.DidNotReceive().WriteLogDataDBCUSTAsync(logData);
    }

    [Fact]
    public async Task WriteLogData_Should_CallWriteLogDataDBCUSTAsync_WhenWriteLogDataDBLOGAsyncThrowException()
    {
        // Arrange
        var logData = _fixture.Create<LogData>();
        var processName = _fixture.Create<string>();
        _logDbRepository.WriteLogDataDBLOGAsync(Arg.Any<LogData>()).Throws<Exception>();

        // Action
        _backgroundTask.WriteLogData(processName, logData);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _logDbRepository.Received(1).WriteLogDataDBLOGAsync(logData);
        await _logDbRepository.Received(1).WriteLogDataDBCUSTAsync(logData);
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
        await _logDbRepository.Received(1).WriteLogTrackingDBLOGAsync(logTracking);
    }

    [Fact]
    public async Task WriteLogTracking_Should_LogError_WhenWriteLogTrackingDBLOGAsyncThrowException()
    {
        // Arrange
        var logTracking = _fixture.Create<LogTracking>();
        var processName = _fixture.Create<string>();
        _logDbRepository.WriteLogTrackingDBLOGAsync(Arg.Any<LogTracking>()).Throws<Exception>();

        // Action
        _backgroundTask.WriteLogTracking(processName, logTracking);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _logDbRepository.Received(1).WriteLogTrackingDBLOGAsync(logTracking);
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
        await _commissionRepository.Received(1).InsertDefaultCommAsync(param);
    }

    [Fact]
    public async Task InsertDefaultCommLicenseeDb_Should_LogError_WhenInsertDefaultCommAsyncThrowException()
    {
        // Arrange
        var param = _fixture.Create<InsertDefaultCommissionParam>();
        var processName = _fixture.Create<string>();
        _commissionRepository.InsertDefaultCommAsync(Arg.Any<InsertDefaultCommissionParam>()).Throws<Exception>();

        // Action
        _backgroundTask.InsertDefaultCommLicenseeDb(processName, param);
        await Task.Delay(100); // Delay 100ms during background execution

        // Assert
        await _commissionRepository.Received(1).InsertDefaultCommAsync(param);
        await _logger.Received(1).ErrorAsync(Arg.Is<string>(msg =>
            msg.StartsWith($"ERROR [Background Task]: An exception occurred in the {processName} background process.")));
    }

    #endregion
}