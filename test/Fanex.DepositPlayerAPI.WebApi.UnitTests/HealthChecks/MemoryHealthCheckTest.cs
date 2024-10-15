namespace Fanex.DepositPlayerAPI.WebApi.UnitTests.HealthChecks;

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using WebApi.HealthChecks;

public class MemoryHealthCheckTest
{
    private readonly Fixture _fixture;
    private readonly MemoryHealthCheck _healthChecks;
    private readonly IMemoryCache _memoryCache;

    public MemoryHealthCheckTest()
    {
        _memoryCache = Substitute.For<IMemoryCache>();
        _healthChecks = new MemoryHealthCheck(_memoryCache);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task MemoryHealthCheck_Should_ReturnCorrectData()
    {
        // Arrange
        var status = GC.GetTotalMemory(false) / (1024L * 1024L) < 1024L ? HealthStatus.Healthy : HealthStatus.Degraded;
        var memoryStatistics = _fixture.Create<MemoryCacheStatistics>();
        _memoryCache.GetCurrentStatistics().Returns(memoryStatistics);

        // Action
        var result = await _healthChecks.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(status);
        result.Description.Should().Be("Reports degraded status if allocated bytes >= 1024 mbs.");
        result.Data["Memory Cache"].Should().BeEquivalentTo(memoryStatistics);
        result.Exception.Should().BeNull();
    }
}