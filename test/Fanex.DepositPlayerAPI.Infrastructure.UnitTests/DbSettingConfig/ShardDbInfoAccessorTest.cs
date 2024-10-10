namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.DbSettingConfig;

using FluentAssertions;
using Infrastructure.DbSettingConfig;
using Microsoft.AspNetCore.Http;
using NSubstitute;

public class ShardDbInfoAccessorTest
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShardDbInfoAccessor _shardDbInfoAccessor;

    public ShardDbInfoAccessorTest()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _shardDbInfoAccessor = new ShardDbInfoAccessor(_httpContextAccessor);
    }

    [Fact]
    public void ToShardCustDbSp_Should_AddPrefixShardCustDBToTheInputKey()
    {
        // Arrange
        var key = "OriginalKey";

        // Action
        var result = _shardDbInfoAccessor.ToShardCustDbSp(key);

        // Assert
        result.Should().Be("ShardCustDB_OriginalKey");
    }

    [Fact]
    public void ToShardSiteDbSp_Should_AddPrefixShardSiteDBToTheInputKey()
    {
        // Arrange
        var key = "OriginalKey";

        // Action
        var result = _shardDbInfoAccessor.ToShardSiteDbSp(key);

        // Assert
        result.Should().Be("ShardSiteDB_OriginalKey");
    }

    #region GetSpKey

    [Fact]
    public void GetSpKey_Should_ReturnOriginalKeyWhenItDoesNotStartWithAnyPrefix()
    {
        // Arrange
        var key = "OriginalKey";

        // Action
        var result = _shardDbInfoAccessor.GetSpKey(key);

        // Assert
        result.Should().Be(key);
    }

    [Fact]
    public void GetSpKey_Should_RemovePrefixShardCustDB()
    {
        // Arrange
        var key = "ShardCustDB_123";

        // Action
        var result = _shardDbInfoAccessor.GetSpKey(key);

        // Assert
        result.Should().Be("123");
    }

    [Fact]
    public void GetSpKey_Should_RemovePrefixShardSiteDB()
    {
        // Arrange
        var key = "ShardSiteDB_123";

        // Action
        var result = _shardDbInfoAccessor.GetSpKey(key);

        // Assert
        result.Should().Be("123");
    }

    #endregion

    #region TryUseShardDb

    [Fact]
    public void TryUseShardDb_Should_ReturnFalseWhenKeyIsEmpty()
    {
        // Arrange
        var key = string.Empty;

        // Action
        var result = _shardDbInfoAccessor.TryUseShardDb(key, out var dbServer);

        // Assert
        result.Should().BeFalse();
        dbServer.Should().BeNull();
    }

    [Fact]
    public void TryUseShardDb_Should_ReturnFalseWhenKeyDoesNotStartWithPrefixForShardCustDB()
    {
        // Arrange
        var key = "NotShardCustDB_123";

        // Action
        var result = _shardDbInfoAccessor.TryUseShardDb(key, out var dbServer);

        // Assert
        result.Should().BeFalse();
        dbServer.Should().BeNull();
    }

    [Fact]
    public void TryUseShardDb_Should_ReturnFalseWhenKeyDoesNotStartWithPrefixForShardSiteDB()
    {
        // Arrange
        var key = "NotShardSiteDB_123";

        // Action
        var result = _shardDbInfoAccessor.TryUseShardDb(key, out var dbServer);

        // Assert
        result.Should().BeFalse();
        dbServer.Should().BeNull();
    }

    [Fact]
    public void TryUseShardDb_Should_ReturnTrueAndSetDbServerToCustDbServerWhenKeyStartWithPrefixForShardCustDB()
    {
        // Arrange
        var key = "ShardCustDB_123";
        _httpContextAccessor.HttpContext!.Items.TryGetValue(Arg.Any<string>(), out _).Returns(x =>
        {
            x[1] = "CustDbServer123";
            return true;
        });

        // Action
        var result = _shardDbInfoAccessor.TryUseShardDb(key, out var dbServer);

        // Assert
        result.Should().BeTrue();
        dbServer.Should().Be("CustDbServer123");
    }

    [Fact]
    public void TryUseShardDb_Should_ReturnTrueAndSetDbServerToSiteDbServerWhenKeyStartWithPrefixForShardSiteDB()
    {
        // Arrange
        var key = "ShardSiteDB_123";
        _httpContextAccessor.HttpContext!.Items.TryGetValue(Arg.Any<string>(), out _).Returns(x =>
        {
            x[1] = "SiteDbServer123";
            return true;
        });

        // Action
        var result = _shardDbInfoAccessor.TryUseShardDb(key, out var dbServer);

        // Assert
        result.Should().BeTrue();
        dbServer.Should().Be("SiteDbServer123");
    }

    #endregion
}