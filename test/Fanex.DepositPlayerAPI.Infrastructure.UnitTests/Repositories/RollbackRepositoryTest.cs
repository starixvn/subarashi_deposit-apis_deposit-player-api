namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using AutoFixture;
using Constants;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using NSubstitute;

[Collection("DbFactoryFixture")]
public class RollbackRepositoryTest : RepositoryBaseTest
{
    private readonly IRollbackRepository _rollbackRepository;

    public RollbackRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _rollbackRepository = new RollbackRepository(dbFactoryFixture, ShardDbInfoAccessor);
    }

    [Fact]
    public async Task RollbackNewPlayerAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var custId = Fixture.Create<int>();

        // Action
        await _rollbackRepository.RollbackNewPlayerAsync(custId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, custId);
    }

    [Fact]
    public async Task RollbackNewPlayerSettingAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _rollbackRepository.RollbackNewPlayerSettingAsync(custId);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task RollbackNewPlayerSettingAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var custId = Fixture.Create<int>();

        // Action
        await _rollbackRepository.RollbackNewPlayerSettingAsync(custId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, custId);
        ObjectDb.Received(1).SetParameterValue("RoleId", 1);
    }

    [Fact]
    public async Task RollbackNewPlayerUserInfoAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var custId = Fixture.Create<int>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _rollbackRepository.RollbackNewPlayerUserInfoAsync(custId);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task RollbackNewPlayerUserInfoAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var custId = Fixture.Create<int>();

        // Action
        await _rollbackRepository.RollbackNewPlayerUserInfoAsync(custId);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, custId);
    }
}