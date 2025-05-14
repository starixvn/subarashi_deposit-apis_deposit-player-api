namespace Fanex.DepositPlayerAPI.WebApi.UnitTests.HealthChecks;

using Data.Vault;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using WebApi.HealthChecks;

public class VaultHealthCheckTest
{
    private static readonly Dictionary<string, string> DbAliasTest = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Test1", "0.0.0.1" },
        { "Test2", "0.0.1.1" },
        { "Test3", "0.1.1.1" },
        { "Test4", "1.1.1.1" }
    };

    private readonly IConfiguration _configuration;
    private readonly VaultDbAliasService _dbAliasService;
    private readonly VaultHealthCheck _healthChecks;

    public VaultHealthCheckTest()
    {
        _configuration = Substitute.For<IConfiguration>();
        _dbAliasService = Substitute.For<VaultDbAliasService>();
        _healthChecks = new VaultHealthCheck(_configuration, _dbAliasService);

        var configurationSections = new List<IConfigurationSection>();
        foreach (var alias in DbAliasTest)
        {
            var mockSection = Substitute.For<IConfigurationSection>();
            mockSection.Key.Returns(alias.Key);
            configurationSections.Add(mockSection);
        }

        _configuration.GetSection("ConnectionStrings").GetChildren().Returns(configurationSections);
    }

    [Fact]
    public async Task VaultHealthCheck_Should_ReturnCorrectData()
    {
        // Arrange
        foreach (var pair in DbAliasTest)
        {
            _dbAliasService.GetIp(pair.Key).Returns(pair.Value);
        }

        // Action
        var result = await _healthChecks.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Vault Info");
        result.Exception.Should().BeNull();
        result.Data.Should().BeEquivalentTo(DbAliasTest);
    }
}