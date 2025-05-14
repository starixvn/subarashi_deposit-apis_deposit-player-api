namespace Fanex.DepositPlayerAPI.WebApi.UnitTests.HostedServices;

using Application.Constants;
using Application.Services;
using Domain.Entities;
using Domain.Repositories;
using Logging;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using WebApi.HostedServices;

public class SetupDbServerConfigCacheHostedServiceTest
{
    private readonly ICacheProvider _cacheProvider;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SetupDbServerConfigCacheHostedService _hostedService;
    private readonly ILogger _logger;
    private readonly IServerDbConfigRepository _serverDbConfigRepository;

    public SetupDbServerConfigCacheHostedServiceTest()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        _serverDbConfigRepository = Substitute.For<IServerDbConfigRepository>();
        _cacheProvider = Substitute.For<ICacheProvider>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _logger = Substitute.For<ILogger>();

        var serviceScope = Substitute.For<IServiceScope>();
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();

        serviceScopeFactory.CreateScope().Returns(serviceScope);
        serviceScope.ServiceProvider.Returns(serviceProvider);

        serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(serviceScopeFactory);
        serviceProvider.GetService(typeof(IServerDbConfigRepository)).Returns(_serverDbConfigRepository);
        serviceProvider.GetService(typeof(ICacheProvider)).Returns(_cacheProvider);
        serviceProvider.GetService(typeof(IDateTimeProvider)).Returns(_dateTimeProvider);
        serviceProvider.GetService(typeof(ILogger)).Returns(_logger);

        _hostedService = new SetupDbServerConfigCacheHostedService(serviceProvider);
    }

    [Fact]
    public async Task StartAsync_ShouldLogSuccess_WhenCacheSetupIsSuccessful()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var serverDbConfigs = new Dictionary<int, ServerDbConfig> { { 1, new ServerDbConfig() } };
        _cacheProvider.GetOrCreateFromCacheWithoutExpiredAsync(
            CacheKeys.ServerDbConfigAll,
            _serverDbConfigRepository.GetAllServerDbConfigAsync).Returns(Task.FromResult(serverDbConfigs));
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        // Act
        await _hostedService.StartAsync(cancellationToken);

        // Assert
        await _logger.Received().InfoAsync(Arg.Is<string>(s => s.Contains("SUCCESS: Cache setup successful")));
    }

    [Fact]
    public async Task StartAsync_ShouldLogCritical_WhenCacheSetupFailsAfterMaxAttempts()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        _cacheProvider.GetOrCreateFromCacheWithoutExpiredAsync(
            CacheKeys.ServerDbConfigAll,
            _serverDbConfigRepository.GetAllServerDbConfigAsync).Returns(Task.FromResult(new Dictionary<int, ServerDbConfig>()));
        _dateTimeProvider.UtcNow.Returns(DateTime.UtcNow);

        // Act
        await _hostedService.StartAsync(cancellationToken);

        // Assert
        await _logger.Received().CriticalAsync(Arg.Is<string>(s => s.Contains("ERROR: Cache setup failed after 3 attempts")));
    }
}