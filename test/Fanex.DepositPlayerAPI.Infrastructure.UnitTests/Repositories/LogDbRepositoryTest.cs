namespace Fanex.DepositPlayerAPI.Infrastructure.UnitTests.Repositories;

using AutoFixture;
using Constants;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Repositories;
using NSubstitute;

[Collection("DbFactoryFixture")]
public class LogDbRepositoryTest : RepositoryBaseTest
{
    private readonly ILogDbRepository _logDbRepository;

    public LogDbRepositoryTest(DbFactoryFixture dbFactoryFixture) : base(dbFactoryFixture)
    {
        _logDbRepository = new LogDbRepository(dbFactoryFixture, ShardDbInfoAccessor);
    }

    [Fact]
    public async Task WriteLogDataDBLOGAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var logData = Fixture.Create<LogData>();

        // Action
        await _logDbRepository.WriteLogDataDBLOGAsync(logData);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.UserId, logData.UserId);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, logData.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.ActionId, logData.ActionId);
        ObjectDb.Received(1).SetParameterValue(DbParam.TrackingValue, logData.TrackingValue);
        ObjectDb.Received(1).SetParameterValue(DbParam.TrackingValue2, logData.TrackingValue2);
    }

    [Fact]
    public async Task WriteLogDataDBCUSTAsync_Should_CreateCorrectShardSpKey()
    {
        // Arrange
        var logData = Fixture.Create<LogData>();
        var shardSpKey = Fixture.Create<string>();
        ShardDbInfoAccessor.ToShardCustDbSp(Arg.Any<string>()).Returns(shardSpKey);

        // Action
        await _logDbRepository.WriteLogDataDBCUSTAsync(logData);

        // Assert
        ObjectDb.SettingKey.Should().Be(shardSpKey);
    }

    [Fact]
    public async Task WriteLogDataDBCUSTAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var logData = Fixture.Create<LogData>();

        // Action
        await _logDbRepository.WriteLogDataDBCUSTAsync(logData);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.UserId, logData.UserId);
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, logData.CustId);
        ObjectDb.Received(1).SetParameterValue(DbParam.ActionId, logData.ActionId);
        ObjectDb.Received(1).SetParameterValue(DbParam.TrackingValue, logData.TrackingValue);
        ObjectDb.Received(1).SetParameterValue(DbParam.TrackingValue2, logData.TrackingValue2);
    }

    [Fact]
    public async Task WriteLogTrackingDBLOGAsync_Should_SetParameterValueCorrect()
    {
        // Arrange
        var logTracking = Fixture.Create<LogTracking>();

        // Action
        await _logDbRepository.WriteLogTrackingDBLOGAsync(logTracking);

        // Assert
        ObjectDb.Received(1).SetParameterValue(DbParam.CustId, logTracking.CustId);
        ObjectDb.Received(1).SetParameterValue("Action", logTracking.Action);
        ObjectDb.Received(1).SetParameterValue("Status", logTracking.Status);
        ObjectDb.Received(1).SetParameterValue("Message", logTracking.Message);
    }
}